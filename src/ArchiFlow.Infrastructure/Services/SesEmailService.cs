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
        _sourceEmail = configuration["AWS_SOURCE_EMAIL"] ?? throw new ArgumentException("Configuração AWS_SOURCE_EMAIL não encontrada.");
        _sesClient = sesClient ?? (IAmazonSimpleEmailService)Activator.CreateInstance(typeof(AmazonSimpleEmailServiceClient))!;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
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
                        Data = body
                    }
                }
            }
        };

        await _sesClient.SendEmailAsync(sendRequest);
    }
}
