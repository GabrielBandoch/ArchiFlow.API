using System;
using System.IO;

namespace ArchiFlow.Application.Arquivos.Commands;

public record UploadArquivoCommand(
    Guid ProjetoId,
    string FileName,
    string ContentType,
    long Length,
    Stream Stream,
    bool VisivelCliente
);
