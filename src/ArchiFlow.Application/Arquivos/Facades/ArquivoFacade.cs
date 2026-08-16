using ArchiFlow.Application.Arquivos.Commands;
using ArchiFlow.Application.Arquivos.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Arquivos.Facades;

public class ArquivoFacade : IArquivoFacade
{
    private readonly IArquivoService _service;

    public ArquivoFacade(IArquivoService service) => _service = service;

    public Task<IEnumerable<ArquivoDto>> GetByProjetoId(Guid projetoId) => _service.GetByProjetoId(projetoId);
    public Task<ArquivoDto> Upload(UploadArquivoCommand command) => _service.Upload(command);
    public Task Delete(Guid id) => _service.Delete(id);
}
