using ArchiFlow.Application.Fornecedores.Commands;
using ArchiFlow.Application.Fornecedores.DTOs;
using ArchiFlow.Domain.Fornecedores;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Projetos.Enum;
using ArchiFlow.Domain.Shared;

namespace ArchiFlow.Application.Fornecedores.Services;

public class FornecedorService : IFornecedorService
{
    private readonly IFornecedorRepository _repository;
    private readonly IProjetoRepository _projetoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FornecedorService(
        IFornecedorRepository repository,
        IProjetoRepository projetoRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _projetoRepository = projetoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<FornecedorDto>> ObterTodosAsync(string? especialidade = null)
    {
        var fornecedores = string.IsNullOrWhiteSpace(especialidade) || especialidade.Equals("todos", StringComparison.OrdinalIgnoreCase)
            ? await _repository.ObterTodosComRelacionamentosAsync()
            : await _repository.ObterPorEspecialidadeAsync(especialidade);

        return fornecedores.Select(MapearParaDto);
    }

    public async Task<FornecedorDto?> ObterPorIdAsync(Guid id)
    {
        var fornecedor = await _repository.ObterPorIdComRelacionamentosAsync(id);
        return fornecedor == null ? null : MapearParaDto(fornecedor);
    }

    public async Task<FornecedorDto> CriarAsync(CriarFornecedorCommand command)
    {
        var fornecedor = new Fornecedor
        {
            Id = Guid.NewGuid(),
            Nome = command.Nome.Trim(),
            Especialidade = command.Especialidade.Trim(),
            Email = command.Email?.Trim() ?? string.Empty,
            Telefone = command.Telefone?.Trim(),
            Cidade = command.Cidade?.Trim(),
            Estado = command.Estado?.Trim(),
            Descricao = command.Descricao?.Trim(),
            AvaliacaoMedia = 0.0m,
            TotalAvaliacoes = 0,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        await _repository.Create(fornecedor);
        await _unitOfWork.Commit();
        return MapearParaDto(fornecedor);
    }

    public async Task<FornecedorDto> AtualizarAsync(AtualizarFornecedorCommand command)
    {
        var fornecedor = await _repository.ObterPorIdComRelacionamentosAsync(command.Id)
            ?? throw new KeyNotFoundException($"Fornecedor com ID {command.Id} não encontrado.");

        fornecedor.Nome = command.Nome.Trim();
        fornecedor.Especialidade = command.Especialidade.Trim();
        fornecedor.Email = command.Email?.Trim() ?? string.Empty;
        fornecedor.Telefone = command.Telefone?.Trim();
        fornecedor.Cidade = command.Cidade?.Trim();
        fornecedor.Estado = command.Estado?.Trim();
        fornecedor.Descricao = command.Descricao?.Trim();
        fornecedor.Ativo = command.Ativo;

        await _repository.Update(fornecedor);
        await _unitOfWork.Commit();
        return MapearParaDto(fornecedor);
    }

    public async Task<bool> ExcluirAsync(Guid id)
    {
        var fornecedor = await _repository.GetById(id);
        if (fornecedor == null) return false;

        await _repository.Delete(id);
        await _unitOfWork.Commit();
        return true;
    }

    public async Task<AvaliacaoFornecedorDto> AdicionarAvaliacaoAsync(AdicionarAvaliacaoCommand command)
    {
        var fornecedor = await _repository.ObterPorIdComRelacionamentosAsync(command.FornecedorId)
            ?? throw new KeyNotFoundException($"Fornecedor com ID {command.FornecedorId} não encontrado.");

        if (command.ProjetoId.HasValue)
        {
            _ = await _projetoRepository.GetById(command.ProjetoId.Value)
                ?? throw new KeyNotFoundException($"Projeto com ID {command.ProjetoId.Value} não encontrado.");
        }

        var avaliacao = new AvaliacaoFornecedor
        {
            Id = Guid.NewGuid(),
            FornecedorId = command.FornecedorId,
            ProjetoId = command.ProjetoId,
            Nota = command.Nota,
            Comentario = command.Comentario.Trim(),
            AutorNome = string.IsNullOrWhiteSpace(command.AutorNome) ? "Arquiteto Titular" : command.AutorNome.Trim(),
            DataAvaliacao = DateTime.UtcNow
        };

        await _repository.AdicionarAvaliacaoAsync(avaliacao);
        fornecedor.Avaliacoes.Add(avaliacao);
        fornecedor.RecalcularMedia();
        await _unitOfWork.Commit();

        return new AvaliacaoFornecedorDto
        {
            Id = avaliacao.Id,
            FornecedorId = avaliacao.FornecedorId,
            ProjetoId = avaliacao.ProjetoId,
            Nota = avaliacao.Nota,
            Comentario = avaliacao.Comentario,
            AutorNome = avaliacao.AutorNome,
            DataAvaliacao = avaliacao.DataAvaliacao
        };
    }

    public async Task<ProjetoFornecedorDto> VincularProjetoAsync(VincularProjetoCommand command)
    {
        var fornecedor = await _repository.GetById(command.FornecedorId)
            ?? throw new KeyNotFoundException($"Fornecedor com ID {command.FornecedorId} não encontrado.");

        var projeto = await _projetoRepository.GetById(command.ProjetoId)
            ?? throw new KeyNotFoundException($"Projeto com ID {command.ProjetoId} não encontrado.");

        var vinculo = new ProjetoFornecedor
        {
            Id = Guid.NewGuid(),
            ProjetoId = command.ProjetoId,
            FornecedorId = command.FornecedorId,
            FuncaoNoProjeto = command.FuncaoNoProjeto.Trim(),
            DataVinculo = DateTime.UtcNow,
            Projeto = projeto,
            Fornecedor = fornecedor
        };

        await _repository.AdicionarVinculoProjetoAsync(vinculo);
        await _unitOfWork.Commit();

        return new ProjetoFornecedorDto
        {
            Id = vinculo.Id,
            ProjetoId = vinculo.ProjetoId,
            ProjetoNome = projeto.Nome,
            FornecedorId = vinculo.FornecedorId,
            FornecedorNome = fornecedor.Nome,
            FuncaoNoProjeto = vinculo.FuncaoNoProjeto,
            DataVinculo = vinculo.DataVinculo
        };
    }

    public async Task<bool> DesvincularProjetoAsync(Guid vinculoId)
    {
        var removido = await _repository.RemoverVinculoProjetoAsync(vinculoId);
        if (!removido)
        {
            return false;
        }

        await _unitOfWork.Commit();
        return true;
    }

    public async Task<IEnumerable<ProjetoFornecedorDto>> ObterFornecedoresDoProjetoAsync(Guid projetoId)
    {
        var projeto = await _projetoRepository.GetById(projetoId)
            ?? throw new KeyNotFoundException($"Projeto com ID {projetoId} não encontrado.");

        var vinculos = await _repository.ObterFornecedoresDoProjetoAsync(projetoId);
        return vinculos.Select(v => new ProjetoFornecedorDto
        {
            Id = v.Id,
            ProjetoId = v.ProjetoId,
            ProjetoNome = v.Projeto?.Nome ?? projeto.Nome,
            FornecedorId = v.FornecedorId,
            FornecedorNome = v.Fornecedor?.Nome,
            FuncaoNoProjeto = v.FuncaoNoProjeto,
            DataVinculo = v.DataVinculo
        });
    }

    private static FornecedorDto MapearParaDto(Fornecedor f)
    {
        return new FornecedorDto
        {
            Id = f.Id,
            Nome = f.Nome,
            Especialidade = f.Especialidade,
            Email = f.Email,
            Telefone = f.Telefone,
            Cidade = f.Cidade,
            Estado = f.Estado,
            Descricao = f.Descricao,
            AvaliacaoMedia = f.AvaliacaoMedia,
            TotalAvaliacoes = f.TotalAvaliacoes,
            Ativo = f.Ativo,
            DataCriacao = f.DataCriacao,
            TotalProjetosAtivos = f.ProjetosVinculados?.Count(p => p.Projeto == null || (p.Projeto.Status != StatusProjeto.Concluido && p.Projeto.Status != StatusProjeto.Cancelado)) ?? 0,
            Avaliacoes = f.Avaliacoes != null
                ? f.Avaliacoes.Select(a => new AvaliacaoFornecedorDto
                {
                    Id = a.Id,
                    FornecedorId = a.FornecedorId,
                    ProjetoId = a.ProjetoId,
                    Nota = a.Nota,
                    Comentario = a.Comentario,
                    AutorNome = a.AutorNome,
                    DataAvaliacao = a.DataAvaliacao
                }).OrderByDescending(a => a.DataAvaliacao).ToList()
                : new List<AvaliacaoFornecedorDto>(),
            ProjetosVinculados = f.ProjetosVinculados != null
                ? f.ProjetosVinculados.Select(p => new ProjetoFornecedorDto
                {
                    Id = p.Id,
                    ProjetoId = p.ProjetoId,
                    ProjetoNome = p.Projeto?.Nome,
                    FornecedorId = p.FornecedorId,
                    FornecedorNome = f.Nome,
                    FuncaoNoProjeto = p.FuncaoNoProjeto,
                    DataVinculo = p.DataVinculo
                }).ToList()
                : new List<ProjetoFornecedorDto>()
        };
    }
}
