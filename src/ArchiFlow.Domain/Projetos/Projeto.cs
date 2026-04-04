using ArchiFlow.Domain.Projetos.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ArchiFlow.Domain.Projetos;

[Table("Projetos")]
public class Projeto
{
    [Key]
    [Column("PJT_Id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("PJT_Nome")]
    public string? Nome { get; set; }

    [Column("PJT_Descricao")]
    public string? Descricao { get; set; } 

    [Column("PJT_Status")]
    public StatusProjetoEnum? Status { get; set; }

    [Column("PJT_Tipo")]
    public TipoProjetoEnum? Tipo { get; set; }

    [Column("PJT_Data_Inicio")]
    public DateTime? DataInicio { get; set; }

    [Column("PJT_Data_Prevista_Entrega")]
    public DateTime? DataPrevistaEntrega { get; set; }

    [Column("PJT_Metragem_Total")]
    public decimal? MetragemTotal { get; set; }

    [Column("PJT_Cliente_Id")]
    public Guid? ClienteId { get; set; }

    [Column("PJT_Criado_Em")]
    public DateTime? CriadoEm { get; set; } = DateTime.UtcNow;

    [Column("PJT_Atualizado_Em")]
    public DateTime? AtualizadoEm { get; set; }

    [JsonIgnore]
    public ICollection<EtapaProjeto>? Etapas { get; set; }
}