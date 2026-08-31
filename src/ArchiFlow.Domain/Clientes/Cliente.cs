namespace ArchiFlow.Domain.Clientes;

public class Cliente
{
    public Guid Id { get; set; }
    public Guid? LeadId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? CpfCnpj { get; set; }
    public string? SenhaPortal { get; set; }
    public bool Ativo { get; set; } = true;
    public string? Endereco { get; set; }
    public string? FotoUrl { get; set; }
}
