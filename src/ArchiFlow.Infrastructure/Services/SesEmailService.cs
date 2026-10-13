using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using ArchiFlow.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Services;

public class SesEmailService : IEmailService
{
    private readonly IAmazonSimpleEmailService _sesClient;
    private readonly string _sourceEmail;

    public SesEmailService(IConfiguration configuration, IAmazonSimpleEmailService? sesClient = null)
    {
        _sourceEmail = configuration["AWS_SOURCE_EMAIL"] 
                    ?? configuration["AWS:SourceEmail"]
                    ?? Environment.GetEnvironmentVariable("AWS_SOURCE_EMAIL") 
                    ?? Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") 
                    ?? throw new InvalidOperationException("Remetente de e-mail não configurado. Defina a variável 'AWS_SOURCE_EMAIL' ou 'SMTP_FROM_EMAIL'.");

        _sesClient = sesClient ?? throw new InvalidOperationException("Cliente AmazonSimpleEmailService não injetado.");
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var htmlBody = body.Contains("<html") || body.Contains("<div") || body.Contains("<p")
            ? body
            : $"<div style=\"font-family: sans-serif; font-size: 14px; line-height: 1.6; color: #222;\">{body.Replace("\r\n", "<br/>").Replace("\n", "<br/>")}</div>";

        var sendRequest = new SendEmailRequest
        {
            Source = _sourceEmail,
            Destination = new Destination
            {
                ToAddresses = new List<string> { to }
            },
            Message = new Message
            {
                Subject = new Content(subject),
                Body = new Body
                {
                    Html = new Content
                    {
                        Charset = "UTF-8",
                        Data = htmlBody
                    },
                    Text = new Content
                    {
                        Charset = "UTF-8",
                        Data = body
                    }
                }
            }
        };

        await _sesClient.SendEmailAsync(sendRequest);
    }
}
