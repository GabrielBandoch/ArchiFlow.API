namespace ArchiFlow.Domain.Shared;

public class OpcaoConfiguracao
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Categoria { get; set; } = string.Empty;
    public string Chave { get; set; } = string.Empty;
    public string Rotulo { get; set; } = string.Empty;
    public string? SubRotulo { get; set; }
    public string? Icone { get; set; }
    public string? Cor { get; set; }
    public string? CorFundo { get; set; }
    public int Ordem { get; set; } = 0;
    public bool Ativo { get; set; } = true;
    public string? DadosExtrasJson { get; set; }
}
