using CoinUpAPI.Services.Email;

namespace CoinUpAPI.Tests.TestHelpers;

public sealed class FakeEmailSender : IEmailSender
{
    public sealed record SentEmail(string ToEmail, string Subject, string Body);

    private readonly List<SentEmail> _sent = new();
    public IReadOnlyList<SentEmail> Sent => _sent;

    public Task SendAsync(string toEmail, string subject, string body)
    {
        _sent.Add(new SentEmail(toEmail, subject, body));
        return Task.CompletedTask;
    }
}
