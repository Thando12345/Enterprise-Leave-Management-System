using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using LeaveFlow.Application.Interfaces;

namespace LeaveFlow.Infrastructure.Services;

public class EmailService(IConfiguration config) : IEmailService
{
    public async Task SendAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(config["Email:From"]));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(config["Email:Host"], int.Parse(config["Email:Port"]!), SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(config["Email:Username"], config["Email:Password"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
