using System;

namespace ArchiFlow.Application.Empresa.DTOs;

public record DadosEmpresaDto(
    Guid Id,
    Guid? EscritorioId,
    string NomeEmpresa,
    string? RazaoSocial,
    string? Cnpj,
    string? RegistroProfissional,
    string? Slogan,
    string? Email,
    string? Telefone,
    string? Endereco,
    string? Cidade,
    string? Estado,
    string? Cep,
    string? Website,
    string? LogoUrl,
    string CorPrimaria,
    DateTime AtualizadoEm
);
