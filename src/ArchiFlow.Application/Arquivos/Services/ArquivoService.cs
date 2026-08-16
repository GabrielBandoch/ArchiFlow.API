using ArchiFlow.Application.Arquivos.Commands;
using ArchiFlow.Application.Arquivos.DTOs;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Arquivos.Services;

public class ArquivoService : IArquivoService
{
    private readonly IArquivoRepository _arquivoRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;

    public ArquivoService(
        IArquivoRepository arquivoRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork)
    {
        _arquivoRepository = arquivoRepository;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ArquivoDto>> GetByProjetoId(Guid projetoId)
    {
        var arquivos = await _arquivoRepository.GetByProjetoId(projetoId);
        return arquivos.Select(a => new ArquivoDto(
            a.Id,
            a.ProjetoId,
            a.Nome,
            a.UrlStorage,
            a.Tipo,
            a.VisivelCliente,
            a.CriadoEm
        ));
    }

    public async Task<ArquivoDto> Upload(UploadArquivoCommand command)
    {
        if (command.Stream == null || command.Length == 0)
        {
            throw new ArgumentException("Nenhum arquivo enviado.");
        }

        if (command.Length > 20 * 1024 * 1024)
        {
            throw new ArgumentException("O arquivo excede o limite de tamanho permitido de 20MB.");
        }

        var urlStorage = await _storageService.UploadAsync(command.Stream, command.FileName, command.ContentType);

        var arquivo = new Arquivo
        {
            Id = Guid.NewGuid(),
            ProjetoId = command.ProjetoId,
            Nome = command.FileName,
            UrlStorage = urlStorage,
            Tipo = command.ContentType,
            VisivelCliente = command.VisivelCliente,
            CriadoEm = DateTime.UtcNow
        };

        await _arquivoRepository.Create(arquivo);
        await _unitOfWork.Commit();

        return new ArquivoDto(
            arquivo.Id,
            arquivo.ProjetoId,
            arquivo.Nome,
            arquivo.UrlStorage,
            arquivo.Tipo,
            arquivo.VisivelCliente,
            arquivo.CriadoEm
        );
    }

    public async Task Delete(Guid id)
    {
        var arquivo = await _arquivoRepository.GetById(id)
            ?? throw new KeyNotFoundException($"Arquivo {id} não encontrado.");

        await _storageService.DeleteAsync(arquivo.UrlStorage);
        await _arquivoRepository.Delete(id);
        await _unitOfWork.Commit();
    }
}
