using System.Net.Mail;
using BankRUs.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace BankRUs.Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _opt;

    public SmtpEmailSender(IOptions<EmailOptions> options)
    {
        _opt = options.Value;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_opt.FromAddress, _opt.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        message.To.Add(toEmail);

        using var client = new SmtpClient(_opt.Host, _opt.Port)
        {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        await client.SendMailAsync(message);
    }
}