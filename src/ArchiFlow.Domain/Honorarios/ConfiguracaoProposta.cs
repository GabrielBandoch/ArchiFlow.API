using System;

namespace ArchiFlow.Domain.Honorarios;

public class ConfiguracaoProposta
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }

    // Identidade do Escritório
    public string NomeEscritorio { get; set; } = string.Empty;
    public string Slogan { get; set; } = string.Empty;
    public string RegistroProfissional { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string CorPrimaria { get; set; } = "#765538";

    // Seções Ativas
    public bool ExibirCabecalho { get; set; } = true;
    public bool ExibirResumo { get; set; } = true;
    public bool ExibirTabelaEtapas { get; set; } = true;
    public bool ExibirMemoriaCalculo { get; set; } = false;
    public bool ExibirCondicoesPagamento { get; set; } = true;
    public bool ExibirTermosGerais { get; set; } = true;
    public bool ExibirAssinaturas { get; set; } = true;

    // Textos e Cláusulas
    public string TextoApresentacao { get; set; } = string.Empty;
    public int ValidadeDias { get; set; } = 15;
    public string CondicoesPagamentoPadrao { get; set; } = string.Empty;
    public string? ChavePix { get; set; }
    public string? DadosBancarios { get; set; }
    public string TermosGerais { get; set; } = string.Empty;
    public string TemplateMensagemWhatsapp { get; set; } = string.Empty;

    public bool Configurado { get; set; } = false;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
}
