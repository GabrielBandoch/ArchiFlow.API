using System;

namespace ArchiFlow.Application.Honorarios.DTOs;

public record ConfiguracaoPropostaDto(
    Guid? Id,
    string NomeEscritorio,
    string Slogan,
    string RegistroProfissional,
    string Email,
    string Telefone,
    string Endereco,
    string? LogoUrl,
    string CorPrimaria,
    bool ExibirCabecalho,
    bool ExibirResumo,
    bool ExibirTabelaEtapas,
    bool ExibirMemoriaCalculo,
    bool ExibirCondicoesPagamento,
    bool ExibirTermosGerais,
    bool ExibirAssinaturas,
    string TextoApresentacao,
    int ValidadeDias,
    string CondicoesPagamentoPadrao,
    string? ChavePix,
    string? DadosBancarios,
    string TermosGerais,
    string TemplateMensagemWhatsapp,
    bool Configurado,
    DateTime? AtualizadoEm
);

public record SalvarConfiguracaoPropostaCommand(
    string NomeEscritorio,
    string? Slogan,
    string? RegistroProfissional,
    string Email,
    string Telefone,
    string? Endereco,
    string? LogoUrl,
    string? CorPrimaria,
    bool ExibirCabecalho,
    bool ExibirResumo,
    bool ExibirTabelaEtapas,
    bool ExibirMemoriaCalculo,
    bool ExibirCondicoesPagamento,
    bool ExibirTermosGerais,
    bool ExibirAssinaturas,
    string TextoApresentacao,
    int ValidadeDias,
    string CondicoesPagamentoPadrao,
    string? ChavePix,
    string? DadosBancarios,
    string TermosGerais,
    string TemplateMensagemWhatsapp
);
