namespace BankRUs.Infrastructure.Email;

public sealed class EmailOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 25;
    public string FromAddress { get; set; } = "no-reply@bankrus.local";
    public string FromName { get; set; } = "BankRUs";
}