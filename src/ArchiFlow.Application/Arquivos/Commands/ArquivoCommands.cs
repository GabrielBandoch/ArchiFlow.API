using Microsoft.AspNetCore.Http;
using System;

namespace ArchiFlow.Application.Arquivos.Commands;

public record UploadArquivoCommand(
    Guid ProjetoId,
    IFormFile? File,
    bool VisivelCliente
);
