using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ArchiFlow.Domain.Projetos.Enum;

namespace ArchiFlow.Domain.Projetos;

[Table("Etapas_Projeto")]
public class EtapaProjeto
{
    [Key]
    [Column("ETo_Id")]
    public Guid Id { get; set; }

    [Column("ETo_Projeto_Id")]
    public Guid? ProjetoId { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("ETo_Nome")]
    public string? Nome { get; set; }

    [Column("ETo_Descricao")]
    public string? Descricao { get; set; }

    [Column("ETo_Status")]
    public StatusEtapaEnum? Status { get; set; }

    [Column("ETo_Ordem")]
    public int? Ordem { get; set; }

    [Column("dETo_Data_Conclusao")]
    public DateTime? DataConclusao { get; set; }

    [JsonIgnore]
    [ForeignKey("ProjetoId")]
    public virtual Projeto? Projeto { get; set; }
}