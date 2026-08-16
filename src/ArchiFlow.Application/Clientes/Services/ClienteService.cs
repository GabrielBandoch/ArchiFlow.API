using ArchiFlow.Application.Clientes.Commands;
using ArchiFlow.Application.Clientes.DTOs;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Leads;
using ArchiFlow.Domain.Leads.Enum;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ArchiFlow.Application.Clientes.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly ILeadRepository    _leadRepository;
    private readonly IProjetoRepository _projetoRepository;
    private readonly IEmailService      _emailService;
    private readonly IUnitOfWork        _unitOfWork;
    private readonly IConfiguration     _configuration;

    public ClienteService(
        IClienteRepository clienteRepository,
        ILeadRepository leadRepository,
        IProjetoRepository projetoRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _clienteRepository = clienteRepository;
        _leadRepository    = leadRepository;
        _projetoRepository = projetoRepository;
        _emailService      = emailService;
        _unitOfWork        = unitOfWork;
        _configuration     = configuration;
    }

    public async Task<IEnumerable<ClienteDto>> GetAll()
    {
        var clientes = await _clienteRepository.GetAll();
        var dtos = new List<ClienteDto>();

        foreach (var c in clientes)
        {
            var projetos = await _projetoRepository.GetByClienteId(c.Id);
            dtos.Add(new ClienteDto(
                c.Id,
                c.LeadId,
                c.Nome,
                c.Email,
                c.Telefone,
                c.CpfCnpj,
                c.Endereco,
                c.Ativo,
                projetos.Count(),
                c.FotoUrl
            ));
        }

        return dtos;
    }

    public async Task<ClienteDto?> GetById(Guid id)
    {
        var c = await _clienteRepository.GetById(id);
        if (c == null) return null;

        var projetos = await _projetoRepository.GetByClienteId(c.Id);
        return new ClienteDto(
            c.Id,
            c.LeadId,
            c.Nome,
            c.Email,
            c.Telefone,
            c.CpfCnpj,
            c.Endereco,
            c.Ativo,
            projetos.Count(),
            c.FotoUrl
        );
    }

    public async Task<ConversaoClienteResponseDto> ConvertLead(ConvertLeadToClienteCommand command)
    {
        var lead = await _leadRepository.GetByIdWithHistorico(command.LeadId)
            ?? throw new KeyNotFoundException($"Lead {command.LeadId} não encontrado.");

        var existingCliente = await _clienteRepository.GetByEmail(lead.Email.Trim());
        if (existingCliente != null)
        {
            throw new ArgumentException("Este e-mail já está cadastrado para outro cliente.");
        }

        var random = new Random();
        var tempPassword = $"Arch@{random.Next(1000, 9999)}";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword, workFactor: 12);

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            Nome = lead.Nome,
            Email = lead.Email,
            Telefone = command.Telefone ?? lead.Telefone,
            CpfCnpj = command.CpfCnpj,
            Endereco = command.Endereco,
            FotoUrl = command.FotoUrl,
            SenhaPortal = passwordHash,
            Ativo = true
        };

        lead.Status = StatusLead.Convertido;
        lead.AtualizadoEm = DateTime.UtcNow;

        await _clienteRepository.Create(cliente);
        await _leadRepository.Update(lead);
        await _unitOfWork.Commit();

        var appName = _configuration["APP_NAME"] ?? "Duna";
        var companyName = _configuration["COMPANY_NAME"] ?? "Duna Arquitetura";
        var emailSubject = $"Bem-vindo ao Portal do Cliente - {appName}";
        var emailBody = $@"
            <h2>Olá, {cliente.Nome}!</h2>
            <p>Seu cadastro como cliente no {appName} foi concluído com sucesso.</p>
            <p>A partir de agora, você pode acompanhar todas as etapas, cronograma e arquivos do seu projeto em tempo real.</p>
            <h3>Suas credenciais de acesso:</h3>
            <p><strong>E-mail:</strong> {cliente.Email}</p>
            <p><strong>Senha Temporária:</strong> {tempPassword}</p>
            <p>Recomendamos alterar sua senha no primeiro acesso.</p>
            <p>Atenciosamente,<br/>Equipe {companyName}</p>";

        try
        {
            await _emailService.SendEmailAsync(cliente.Email, emailSubject, emailBody);
        }
        catch (Exception)
        {
        }

        var dto = new ClienteDto(
            cliente.Id,
            cliente.LeadId,
            cliente.Nome,
            cliente.Email,
            cliente.Telefone,
            cliente.CpfCnpj,
            cliente.Endereco,
            cliente.Ativo,
            0,
            cliente.FotoUrl
        );

        return new ConversaoClienteResponseDto(dto, tempPassword);
    }

    public async Task<ClienteDto> Update(AtualizarClienteCommand command)
    {
        var c = await _clienteRepository.GetById(command.Id)
            ?? throw new KeyNotFoundException($"Cliente {command.Id} não encontrado.");

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new ArgumentException("E-mail é obrigatório.");
        }

        var existing = await _clienteRepository.GetByEmail(command.Email.Trim());
        if (existing != null && existing.Id != command.Id)
        {
            throw new ArgumentException("Este e-mail já está cadastrado para outro cliente.");
        }

        c.Nome = command.Nome;
        c.Email = command.Email;
        c.Telefone = command.Telefone;
        c.CpfCnpj = command.CpfCnpj;
        c.Endereco = command.Endereco;
        if (command.FotoUrl != null)
        {
            c.FotoUrl = command.FotoUrl == "DELETE" ? null : command.FotoUrl;
        }

        await _clienteRepository.Update(c);
        await _unitOfWork.Commit();

        var projetos = await _projetoRepository.GetByClienteId(c.Id);
        return new ClienteDto(
            c.Id,
            c.LeadId,
            c.Nome,
            c.Email,
            c.Telefone,
            c.CpfCnpj,
            c.Endereco,
            c.Ativo,
            projetos.Count(),
            c.FotoUrl
        );
    }

    public async Task<ClienteDto> UpdatePortalAccess(AtualizarPortalAccessCommand command)
    {
        var c = await _clienteRepository.GetById(command.Id)
            ?? throw new KeyNotFoundException($"Cliente {command.Id} não encontrado.");

        c.Ativo = command.Ativo;

        await _clienteRepository.Update(c);
        await _unitOfWork.Commit();

        var projetos = await _projetoRepository.GetByClienteId(c.Id);
        return new ClienteDto(
            c.Id,
            c.LeadId,
            c.Nome,
            c.Email,
            c.Telefone,
            c.CpfCnpj,
            c.Endereco,
            c.Ativo,
            projetos.Count(),
            c.FotoUrl
        );
    }
}
