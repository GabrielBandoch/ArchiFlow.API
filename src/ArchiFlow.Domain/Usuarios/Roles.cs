namespace ArchiFlow.Domain.Usuarios;

public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Gerente = "Gerente";
    public const string Colaborador = "Colaborador";
    public const string Cliente = "Cliente";

    public const string ArquitetoAdmin = "ArquitetoAdmin";
    public const string ArquitetoColaborador = "ArquitetoColaborador";
    public const string Estagiario = "Estagiario";
    public const string Financeiro = "Financeiro";

    public static readonly string[] RolesValidasEquipe =
    [
        Administrador,
        Gerente,
        Colaborador,
        ArquitetoAdmin,
        ArquitetoColaborador,
        Estagiario,
        Financeiro
    ];
}
