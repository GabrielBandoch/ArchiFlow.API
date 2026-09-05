using ArchiFlow.Application.Arquivos.Commands;
using ArchiFlow.Application.Arquivos.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Interfaces.Facades;

public interface IArquivoFacade
{
    Task<IEnumerable<ArquivoDto>> GetByProjetoId(Guid projetoId, bool apenasVisiveisCliente = false);
    Task<ArquivoDto> Upload(UploadArquivoCommand command);
    Task Delete(Guid id);
}
