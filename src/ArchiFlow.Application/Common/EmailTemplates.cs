using System.Net;

namespace ArchiFlow.Application.Common;

public static class EmailTemplates
{
    private const string BaseUrlPadrao = "https://staging.cotu.com.br";

    public static string GerarConviteEquipe(
        string nomeDestinatario, 
        string nomeConvidante, 
        string email, 
        string senhaTemporaria, 
        string? loginUrl = null)
    {
        var url = string.IsNullOrWhiteSpace(loginUrl) ? $"{BaseUrlPadrao}/login" : loginUrl;
        var nomeSeguro = WebUtility.HtmlEncode(nomeDestinatario);
        var convidanteSeguro = WebUtility.HtmlEncode(nomeConvidante);
        var emailSeguro = WebUtility.HtmlEncode(email);
        var senhaSegura = WebUtility.HtmlEncode(senhaTemporaria);

        return $@"<!DOCTYPE html>
<html lang=""pt-BR"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Convite para a Equipe - Cotu</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f1f5f9; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; -webkit-font-smoothing: antialiased;"">
    <table role=""presentation"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f1f5f9; padding: 40px 15px;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""max-width: 580px; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05); border: 1px solid #e2e8f0;"">
                    <!-- Accent Bar -->
                    <tr>
                        <td height=""4"" style=""background-color: #2563eb; line-height: 4px; font-size: 4px;"">&nbsp;</td>
                    </tr>
                    <!-- Header -->
                    <tr>
                        <td style=""padding: 32px 40px 24px; text-align: left; background-color: #0f172a;"">
                            <span style=""font-size: 26px; font-weight: 800; letter-spacing: -0.5px; color: #ffffff;"">COTU</span>
                            <span style=""display: block; font-size: 11px; color: #94a3b8; font-weight: 500; text-transform: uppercase; letter-spacing: 1.5px; margin-top: 4px;"">PLATAFORMA DE GEST&Atilde;O PARA ARQUITETURA</span>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style=""padding: 36px 40px 24px;"">
                            <h1 style=""margin: 0 0 16px; font-size: 20px; font-weight: 700; color: #0f172a; line-height: 1.3;"">
                                Ol&aacute;, {nomeSeguro}!
                            </h1>
                            <p style=""margin: 0 0 24px; font-size: 15px; color: #475569; line-height: 1.6;"">
                                Voc&ecirc; foi convidado por <strong style=""color: #0f172a;"">{convidanteSeguro}</strong> para fazer parte da equipe na plataforma <strong style=""color: #0f172a;"">Cotu</strong>.
                            </p>

                            <!-- Credentials Box -->
                            <table role=""presentation"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; margin-bottom: 28px;"">
                                <tr>
                                    <td style=""padding: 20px 24px;"">
                                        <div style=""font-size: 12px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px; color: #64748b; margin-bottom: 12px;"">
                                            Suas credenciais de primeiro acesso
                                        </div>
                                        <div style=""font-size: 14px; color: #334155; margin-bottom: 8px;"">
                                            <strong style=""color: #0f172a;"">E-mail:</strong> {emailSeguro}
                                        </div>
                                        <div style=""font-size: 14px; color: #334155;"">
                                            <strong style=""color: #0f172a;"">Senha tempor&aacute;ria:</strong>&nbsp;
                                            <code style=""background-color: #e2e8f0; color: #0f172a; font-family: 'Courier New', Courier, monospace; font-size: 15px; font-weight: 700; padding: 4px 10px; border-radius: 4px; letter-spacing: 0.5px;"">{senhaSegura}</code>
                                        </div>
                                    </td>
                                </tr>
                            </table>

                            <!-- Action Button -->
                            <div style=""text-align: center; margin: 32px 0;"">
                                <a href=""{url}"" style=""display: inline-block; background-color: #2563eb; color: #ffffff; text-decoration: none; font-size: 15px; font-weight: 600; padding: 14px 32px; border-radius: 8px; box-shadow: 0 2px 4px rgba(37, 99, 235, 0.2);"" target=""_blank"">
                                    Acessar a Plataforma
                                </a>
                            </div>

                            <!-- Security Tip -->
                            <div style=""background-color: #eff6ff; border-left: 4px solid #3b82f6; padding: 14px 18px; border-radius: 0 6px 6px 0; margin-bottom: 8px;"">
                                <p style=""margin: 0; font-size: 13px; color: #1e40af; line-height: 1.5;"">
                                    <span style=""display: inline-block; background-color: #2563eb; color: #ffffff; font-size: 10px; font-weight: 700; padding: 2px 6px; border-radius: 4px; text-transform: uppercase; margin-right: 6px; vertical-align: middle;"">Dica</span>
                                    <strong>Seguran&ccedil;a:</strong> Ao fazer login pela primeira vez, voc&ecirc; poder&aacute; cadastrar uma nova senha definitiva e personalizada no menu do seu perfil.
                                </p>
                            </div>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style=""padding: 24px 40px; background-color: #f8fafc; border-top: 1px solid #e2e8f0; text-align: center;"">
                            <p style=""margin: 0 0 6px; font-size: 12px; color: #94a3b8;"">
                                &copy; 2026 Cotu Platform. Todos os direitos reservados.
                            </p>
                            <p style=""margin: 0; font-size: 12px; color: #94a3b8;"">
                                <a href=""https://cotu.com.br"" style=""color: #64748b; text-decoration: underline;"" target=""_blank"">cotu.com.br</a> &bull; 
                                <a href=""mailto:contato@cotu.com.br"" style=""color: #64748b; text-decoration: none;"">contato@cotu.com.br</a>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    public static string GerarRedefinicaoSenha(
        string nomeDestinatario, 
        string novaSenha, 
        string? loginUrl = null)
    {
        var url = string.IsNullOrWhiteSpace(loginUrl) ? $"{BaseUrlPadrao}/login" : loginUrl;
        var nomeSeguro = WebUtility.HtmlEncode(nomeDestinatario);
        var senhaSegura = WebUtility.HtmlEncode(novaSenha);

        return $@"<!DOCTYPE html>
<html lang=""pt-BR"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Sua Senha foi Redefinida - Cotu</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f1f5f9; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; -webkit-font-smoothing: antialiased;"">
    <table role=""presentation"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f1f5f9; padding: 40px 15px;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""max-width: 580px; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05); border: 1px solid #e2e8f0;"">
                    <tr>
                        <td height=""4"" style=""background-color: #2563eb; line-height: 4px; font-size: 4px;"">&nbsp;</td>
                    </tr>
                    <tr>
                        <td style=""padding: 32px 40px 24px; text-align: left; background-color: #0f172a;"">
                            <span style=""font-size: 26px; font-weight: 800; letter-spacing: -0.5px; color: #ffffff;"">COTU</span>
                            <span style=""display: block; font-size: 11px; color: #94a3b8; font-weight: 500; text-transform: uppercase; letter-spacing: 1.5px; margin-top: 4px;"">PLATAFORMA DE GEST&Atilde;O PARA ARQUITETURA</span>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 36px 40px 24px;"">
                            <h1 style=""margin: 0 0 16px; font-size: 20px; font-weight: 700; color: #0f172a; line-height: 1.3;"">
                                Ol&aacute;, {nomeSeguro}!
                            </h1>
                            <p style=""margin: 0 0 24px; font-size: 15px; color: #475569; line-height: 1.6;"">
                                Sua senha de acesso ao <strong style=""color: #0f172a;"">Cotu</strong> foi redefinida com sucesso pelo administrador do seu escrit&oacute;rio.
                            </p>

                            <table role=""presentation"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; margin-bottom: 28px;"">
                                <tr>
                                    <td style=""padding: 20px 24px;"">
                                        <div style=""font-size: 12px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px; color: #64748b; margin-bottom: 12px;"">
                                            Sua nova credencial de acesso
                                        </div>
                                        <div style=""font-size: 14px; color: #334155;"">
                                            <strong style=""color: #0f172a;"">Nova senha tempor&aacute;ria:</strong>&nbsp;
                                            <code style=""background-color: #e2e8f0; color: #0f172a; font-family: 'Courier New', Courier, monospace; font-size: 15px; font-weight: 700; padding: 4px 10px; border-radius: 4px; letter-spacing: 0.5px;"">{senhaSegura}</code>
                                        </div>
                                    </td>
                                </tr>
                            </table>

                            <div style=""text-align: center; margin: 32px 0;"">
                                <a href=""{url}"" style=""display: inline-block; background-color: #2563eb; color: #ffffff; text-decoration: none; font-size: 15px; font-weight: 600; padding: 14px 32px; border-radius: 8px; box-shadow: 0 2px 4px rgba(37, 99, 235, 0.2);"" target=""_blank"">
                                    Entrar no Cotu
                                </a>
                            </div>

                            <div style=""background-color: #eff6ff; border-left: 4px solid #3b82f6; padding: 14px 18px; border-radius: 0 6px 6px 0;"">
                                <p style=""margin: 0; font-size: 13px; color: #1e40af; line-height: 1.5;"">
                                    <span style=""display: inline-block; background-color: #2563eb; color: #ffffff; font-size: 10px; font-weight: 700; padding: 2px 6px; border-radius: 4px; text-transform: uppercase; margin-right: 6px; vertical-align: middle;"">Aviso</span>
                                    Recomendamos alterar esta senha logo ap&oacute;s o login na p&aacute;gina do seu perfil. Se voc&ecirc; n&atilde;o solicitou esta redefini&ccedil;&atilde;o, entre em contato imediatamente com o administrador do seu escrit&oacute;rio.
                                </p>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 24px 40px; background-color: #f8fafc; border-top: 1px solid #e2e8f0; text-align: center;"">
                            <p style=""margin: 0 0 6px; font-size: 12px; color: #94a3b8;"">
                                &copy; 2026 Cotu Platform. Todos os direitos reservados.
                            </p>
                            <p style=""margin: 0; font-size: 12px; color: #94a3b8;"">
                                <a href=""https://cotu.com.br"" style=""color: #64748b; text-decoration: underline;"" target=""_blank"">cotu.com.br</a> &bull; 
                                <a href=""mailto:contato@cotu.com.br"" style=""color: #64748b; text-decoration: none;"">contato@cotu.com.br</a>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    public static string GerarAcessoPortalCliente(
        string nomeCliente, 
        string email, 
        string senhaTemporaria, 
        string nomeEscritorio, 
        string? loginUrl = null)
    {
        var url = string.IsNullOrWhiteSpace(loginUrl) ? $"{BaseUrlPadrao}/login" : loginUrl;
        var nomeSeguro = WebUtility.HtmlEncode(nomeCliente);
        var escritorioSeguro = WebUtility.HtmlEncode(nomeEscritorio);
        var emailSeguro = WebUtility.HtmlEncode(email);
        var senhaSegura = WebUtility.HtmlEncode(senhaTemporaria);

        return $@"<!DOCTYPE html>
<html lang=""pt-BR"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Acesso ao Portal do Cliente - {escritorioSeguro}</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f1f5f9; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; -webkit-font-smoothing: antialiased;"">
    <table role=""presentation"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f1f5f9; padding: 40px 15px;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""max-width: 580px; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05); border: 1px solid #e2e8f0;"">
                    <tr>
                        <td height=""4"" style=""background-color: #2563eb; line-height: 4px; font-size: 4px;"">&nbsp;</td>
                    </tr>
                    <tr>
                        <td style=""padding: 32px 40px 24px; text-align: left; background-color: #0f172a;"">
                            <span style=""font-size: 26px; font-weight: 800; letter-spacing: -0.5px; color: #ffffff;"">COTU</span>
                            <span style=""display: block; font-size: 11px; color: #94a3b8; font-weight: 500; text-transform: uppercase; letter-spacing: 1.5px; margin-top: 4px;"">PORTAL DO CLIENTE &bull; {escritorioSeguro}</span>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 36px 40px 24px;"">
                            <h1 style=""margin: 0 0 16px; font-size: 20px; font-weight: 700; color: #0f172a; line-height: 1.3;"">
                                Ol&aacute;, {nomeSeguro}!
                            </h1>
                            <p style=""margin: 0 0 16px; font-size: 15px; color: #475569; line-height: 1.6;"">
                                Seja bem-vindo ao portal de acompanhamento de projetos do escrit&oacute;rio <strong style=""color: #0f172a;"">{escritorioSeguro}</strong>.
                            </p>
                            <p style=""margin: 0 0 24px; font-size: 15px; color: #475569; line-height: 1.6;"">
                                A partir de agora, voc&ecirc; pode acompanhar todas as etapas, cronograma, imagens 3D e arquivos do seu projeto arquitet&ocirc;nico em tempo real.
                            </p>

                            <table role=""presentation"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; margin-bottom: 28px;"">
                                <tr>
                                    <td style=""padding: 20px 24px;"">
                                        <div style=""font-size: 12px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px; color: #64748b; margin-bottom: 12px;"">
                                            Suas credenciais de acesso ao Portal
                                        </div>
                                        <div style=""font-size: 14px; color: #334155; margin-bottom: 8px;"">
                                            <strong style=""color: #0f172a;"">E-mail:</strong> {emailSeguro}
                                        </div>
                                        <div style=""font-size: 14px; color: #334155;"">
                                            <strong style=""color: #0f172a;"">Senha tempor&aacute;ria:</strong>&nbsp;
                                            <code style=""background-color: #e2e8f0; color: #0f172a; font-family: 'Courier New', Courier, monospace; font-size: 15px; font-weight: 700; padding: 4px 10px; border-radius: 4px; letter-spacing: 0.5px;"">{senhaSegura}</code>
                                        </div>
                                    </td>
                                </tr>
                            </table>

                            <div style=""text-align: center; margin: 32px 0;"">
                                <a href=""{url}"" style=""display: inline-block; background-color: #2563eb; color: #ffffff; text-decoration: none; font-size: 15px; font-weight: 600; padding: 14px 32px; border-radius: 8px; box-shadow: 0 2px 4px rgba(37, 99, 235, 0.2);"" target=""_blank"">
                                    Acessar Meu Projeto
                                </a>
                            </div>

                            <div style=""background-color: #eff6ff; border-left: 4px solid #3b82f6; padding: 14px 18px; border-radius: 0 6px 6px 0;"">
                                <p style=""margin: 0; font-size: 13px; color: #1e40af; line-height: 1.5;"">
                                    <span style=""display: inline-block; background-color: #2563eb; color: #ffffff; font-size: 10px; font-weight: 700; padding: 2px 6px; border-radius: 4px; text-transform: uppercase; margin-right: 6px; vertical-align: middle;"">Dica</span>
                                    Salve este link nos seus favoritos para acompanhar as novidades e atualiza&ccedil;&otilde;es do seu projeto.
                                </p>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 24px 40px; background-color: #f8fafc; border-top: 1px solid #e2e8f0; text-align: center;"">
                            <p style=""margin: 0 0 6px; font-size: 12px; color: #94a3b8;"">
                                {escritorioSeguro} &bull; Powered by COTU Platform
                            </p>
                            <p style=""margin: 0; font-size: 12px; color: #94a3b8;"">
                                <a href=""https://cotu.com.br"" style=""color: #64748b; text-decoration: underline;"" target=""_blank"">cotu.com.br</a>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    public static string GerarEmailRecuperacaoSenha(
        string nomeDestinatario, 
        string codigoVerificacao, 
        string? resetUrl = null)
    {
        var url = string.IsNullOrWhiteSpace(resetUrl) ? $"{BaseUrlPadrao}/login" : resetUrl;
        var nomeSeguro = WebUtility.HtmlEncode(nomeDestinatario);
        var codigoSeguro = WebUtility.HtmlEncode(codigoVerificacao);

        return $@"<!DOCTYPE html>
<html lang=""pt-BR"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Recuperação de Senha - COTU</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f1f5f9; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; -webkit-font-smoothing: antialiased;"">
    <table role=""presentation"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f1f5f9; padding: 40px 15px;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""max-width: 580px; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05); border: 1px solid #e2e8f0;"">
                    <tr>
                        <td height=""4"" style=""background-color: #E57A44; line-height: 4px; font-size: 4px;"">&nbsp;</td>
                    </tr>
                    <tr>
                        <td style=""padding: 32px 40px 24px; text-align: left; background-color: #0f172a;"">
                            <span style=""font-size: 26px; font-weight: 800; letter-spacing: -0.5px; color: #ffffff;"">COTU</span>
                            <span style=""display: block; font-size: 11px; color: #94a3b8; font-weight: 500; text-transform: uppercase; letter-spacing: 1.5px; margin-top: 4px;"">RECUPERA&Ccedil;&Atilde;O DE ACESSO &bull; PLATAFORMA DE GEST&Atilde;O</span>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 36px 40px 24px;"">
                            <h1 style=""margin: 0 0 16px; font-size: 20px; font-weight: 700; color: #0f172a; line-height: 1.3;"">
                                Ol&aacute;, {nomeSeguro}!
                            </h1>
                            <p style=""margin: 0 0 24px; font-size: 15px; color: #475569; line-height: 1.6;"">
                                Recebemos uma solicita&ccedil;&atilde;o para redefinir a senha da sua conta no <strong style=""color: #0f172a;"">COTU</strong>. Utilize o c&oacute;digo de verifica&ccedil;&atilde;o abaixo para criar uma nova senha:
                            </p>

                            <table role=""presentation"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #fff7ed; border: 1px solid #ffedd5; border-radius: 8px; margin-bottom: 28px;"">
                                <tr>
                                    <td style=""padding: 24px; text-align: center;"">
                                        <div style=""font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 1px; color: #c2410c; margin-bottom: 8px;"">
                                            C&oacute;digo de Seguran&ccedil;a (V&aacute;lido por 15 minutos)
                                        </div>
                                        <div style=""font-size: 32px; font-weight: 800; letter-spacing: 6px; color: #9a3412; font-family: 'Courier New', Courier, monospace;"">
                                            {codigoSeguro}
                                        </div>
                                    </td>
                                </tr>
                            </table>

                            <div style=""text-align: center; margin: 28px 0;"">
                                <a href=""{url}"" style=""display: inline-block; background-color: #E57A44; color: #ffffff; text-decoration: none; font-size: 15px; font-weight: 600; padding: 14px 32px; border-radius: 8px; box-shadow: 0 2px 4px rgba(229, 122, 68, 0.25);"" target=""_blank"">
                                    Redefinir Minha Senha no COTU
                                </a>
                            </div>

                            <div style=""background-color: #f8fafc; border-left: 4px solid #94a3b8; padding: 14px 18px; border-radius: 0 6px 6px 0;"">
                                <p style=""margin: 0; font-size: 13px; color: #475569; line-height: 1.5;"">
                                    <strong>Importante:</strong> Se voc&ecirc; n&atilde;o solicitou a recupera&ccedil;&atilde;o de senha, desconsidere este e-mail. Sua senha atual permanecer&aacute; segura e inalterada.
                                </p>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 24px 40px; background-color: #f8fafc; border-top: 1px solid #e2e8f0; text-align: center;"">
                            <p style=""margin: 0 0 6px; font-size: 12px; color: #94a3b8;"">
                                &copy; 2026 COTU Platform. Todos os direitos reservados.
                            </p>
                            <p style=""margin: 0; font-size: 12px; color: #94a3b8;"">
                                <a href=""https://cotu.com.br"" style=""color: #64748b; text-decoration: underline;"" target=""_blank"">cotu.com.br</a> &bull; 
                                <a href=""mailto:contato@cotu.com.br"" style=""color: #64748b; text-decoration: none;"">contato@cotu.com.br</a>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}
