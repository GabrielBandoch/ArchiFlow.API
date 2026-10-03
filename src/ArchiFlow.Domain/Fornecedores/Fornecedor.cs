namespace ArchiFlow.Domain.Fornecedores;

public class Fornecedor
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Especialidade { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Descricao { get; set; }
    public decimal AvaliacaoMedia { get; set; } = 5.0m;
    public int TotalAvaliacoes { get; set; } = 0;
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public List<AvaliacaoFornecedor> Avaliacoes { get; set; } = new();
    public List<ProjetoFornecedor> ProjetosVinculados { get; set; } = new();

    public void RecalcularMedia()
    {
        if (Avaliacoes == null || Avaliacoes.Count == 0)
        {
            AvaliacaoMedia = 5.0m;
            TotalAvaliacoes = 0;
            return;
        }

        TotalAvaliacoes = Avaliacoes.Count;
        AvaliacaoMedia = Math.Round((decimal)Avaliacoes.Average(a => a.Nota), 1);
    }
}
