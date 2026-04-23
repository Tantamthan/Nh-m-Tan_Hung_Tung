namespace Lab8_PhamVanTung_2324801030079.Models;

public sealed class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromEmail { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string SecureSocketOption { get; set; } = "StartTls";

    public bool HasValidConfiguration()
    {
        return !string.IsNullOrWhiteSpace(Host)
            && Port > 0
            && !string.IsNullOrWhiteSpace(UserName)
            && !string.IsNullOrWhiteSpace(Password)
            && !string.IsNullOrWhiteSpace(FromEmail)
            && !string.IsNullOrWhiteSpace(DisplayName);
    }
}
