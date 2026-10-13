namespace ArchiFlow.Application.SuperAdmin.DTOs;

public class SuperAdminDashboardKpisDto
{
    public int TotalEscritorios { get; set; }
    public int TotalAdimplentes { get; set; }
    public int TotalEmDebito { get; set; }
    public int TotalSuspensos { get; set; }
    public int TotalTrial { get; set; }
    public decimal MrrTotal { get; set; }
    public decimal TotalEmAtrasoReais { get; set; }
    public decimal TaxaAdimplenciaPercentual { get; set; }
}
