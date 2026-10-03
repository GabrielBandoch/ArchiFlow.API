using System.ComponentModel.DataAnnotations;

namespace ArchiFlow.Application.Agenda.Commands;

public class ConectarGoogleOAuthCommand
{
    [Required(ErrorMessage = "O código de autorização é obrigatório.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "A URI de redirecionamento é obrigatória.")]
    public string RedirectUri { get; set; } = string.Empty;

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }
}
