using Microsoft.AspNetCore.Http;

namespace ArchiFlow.Application.Financeiro.Commands;

public record UploadComprovanteCommand(
    IFormFile? File
);
