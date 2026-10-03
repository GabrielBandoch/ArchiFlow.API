namespace ArchiFlow.Application.Configuracoes.DTOs;

public class OpcaoConfiguracaoDto
{
    public Guid Id { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string Chave { get; set; } = string.Empty;
    public string Rotulo { get; set; } = string.Empty;
    public string? SubRotulo { get; set; }
    public string? Icone { get; set; }
    public string? Cor { get; set; }
    public string? CorFundo { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; }
    public string? DadosExtrasJson { get; set; }
}

public class SalvarOpcaoConfiguracaoCommand
{
    public Guid? Id { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string Chave { get; set; } = string.Empty;
    public string Rotulo { get; set; } = string.Empty;
    public string? SubRotulo { get; set; }
    public string? Icone { get; set; }
    public string? Cor { get; set; }
    public string? CorFundo { get; set; }
    public int Ordem { get; set; }
    public string? DadosExtrasJson { get; set; }
}
