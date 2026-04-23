namespace Lab8_PhamVanTung_2324801030079.Models.Security;

public sealed class CurrentUser
{
    public string UserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = [];

    public bool IsAuthenticated { get; set; }
}
