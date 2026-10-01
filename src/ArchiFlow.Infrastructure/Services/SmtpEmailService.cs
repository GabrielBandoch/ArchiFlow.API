using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using ArchiFlow.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ArchiFlow.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var host = _configuration["SMTP_HOST"] 
            ?? Environment.GetEnvironmentVariable("SMTP_HOST") 
            ?? "smtp.hostinger.com";

        var portStr = _configuration["SMTP_PORT"] 
            ?? Environment.GetEnvironmentVariable("SMTP_PORT") 
            ?? "587";

        var user = _configuration["SMTP_USER"] 
            ?? Environment.GetEnvironmentVariable("SMTP_USER");

        var password = _configuration["SMTP_PASSWORD"] 
            ?? Environment.GetEnvironmentVariable("SMTP_PASSWORD");

        var fromEmail = _configuration["SMTP_FROM_EMAIL"] 
            ?? Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") 
            ?? user;

        var fromName = _configuration["SMTP_FROM_NAME"] 
            ?? Environment.GetEnvironmentVariable("SMTP_FROM_NAME") 
            ?? "ArchiFlow";

        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Configurações de SMTP_USER ou SMTP_PASSWORD não foram preenchidas. O e-mail para '{To}' não foi enviado via SMTP.", to);
            return;
        }

        int port = int.TryParse(portStr, out var p) ? p : 587;

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(user, password),
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail!, fromName),
            Subject = subject
        };
        mailMessage.To.Add(to);

        var isHtml = body.Contains("<html", StringComparison.OrdinalIgnoreCase) 
                  || body.Contains("<div", StringComparison.OrdinalIgnoreCase) 
                  || body.Contains("<p>", StringComparison.OrdinalIgnoreCase) 
                  || body.Contains("<br", StringComparison.OrdinalIgnoreCase);

        if (!isHtml)
        {
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = $@"
<!DOCTYPE html>
<html>
<head><meta charset=""UTF-8""></head>
<body style=""margin: 0; padding: 20px; background-color: #f8fafc; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;"">
  <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);"">
    <tr>
      <td style=""background-color: #0f172a; padding: 24px 32px; text-align: left;"">
        <h1 style=""margin: 0; color: #ffffff; font-size: 22px; font-weight: 700; letter-spacing: -0.5px;"">ArchiFlow</h1>
      </td>
    </tr>
    <tr>
      <td style=""padding: 32px; color: #334155; font-size: 15px; line-height: 1.6;"">
        {WebUtility.HtmlEncode(body).Replace("\n", "<br/>")}
      </td>
    </tr>
    <tr>
      <td style=""background-color: #f1f5f9; padding: 16px 32px; text-align: center; color: #64748b; font-size: 12px;"">
        © {DateTime.UtcNow.Year} ArchiFlow — Gestão Integrada de Escritórios de Arquitetura
      </td>
    </tr>
  </table>
</body>
</html>";
        }
        else
        {
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = body;
        }

        try
        {
            _logger.LogInformation("Enviando e-mail SMTP para {To} usando host {Host}:{Port}...", to, host, port);
            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("E-mail SMTP enviado com sucesso para {To}.", to);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Falha no envio de e-mail via SMTP para {to}.", ex);
        }
    }
}
