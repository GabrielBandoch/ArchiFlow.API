namespace ArchiFlow.Domain.Usuarios;

public static class ModulosApp
{
    public const string Dashboard = "dashboard";
    public const string Leads = "leads";
    public const string Clientes = "clientes";
    public const string Fornecedores = "fornecedores";
    public const string Projetos = "projetos";
    public const string Agenda = "agenda";
    public const string Financeiro = "financeiro";
    public const string Simulador = "simulador";
    public const string Configuracoes = "configuracoes";

    public static readonly string[] Todos =
    [
        Dashboard,
        Leads,
        Clientes,
        Fornecedores,
        Projetos,
        Agenda,
        Financeiro,
        Simulador,
        Configuracoes
    ];
}
