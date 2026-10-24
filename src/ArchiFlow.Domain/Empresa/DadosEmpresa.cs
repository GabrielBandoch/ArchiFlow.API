using System;

namespace ArchiFlow.Domain.Empresa;

public class DadosEmpresa
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? EscritorioId { get; set; }
    public string NomeEmpresa { get; set; } = "COTU Studio";
    public string? RazaoSocial { get; set; }
    public string? Cnpj { get; set; }
    public string? RegistroProfissional { get; set; }
    public string? Slogan { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Endereco { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Cep { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string CorPrimaria { get; set; } = "#765538";
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
}

