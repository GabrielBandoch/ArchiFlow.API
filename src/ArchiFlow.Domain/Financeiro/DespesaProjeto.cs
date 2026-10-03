using ArchiFlow.Domain.Projetos;
using System;

namespace ArchiFlow.Domain.Financeiro;

public class DespesaProjeto
{
    public Guid Id { get; set; }
    public Guid? ProjetoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataDespesa { get; set; }
    public CategoriaDespesa Categoria { get; set; } = CategoriaDespesa.Outros;
    public string? Observacoes { get; set; }
    public string? ComprovanteUrl { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }

    public Projeto? Projeto { get; set; }
}
