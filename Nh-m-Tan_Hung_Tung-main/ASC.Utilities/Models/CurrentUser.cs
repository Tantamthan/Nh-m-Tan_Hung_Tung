namespace ASC.Utilities.Models
{
    public class CurrentUser
    {
        public string Id { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = [];

        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Id);

        public bool IsInRole(string role)
        {
            return Roles.Any(item => string.Equals(item, role, StringComparison.OrdinalIgnoreCase));
        }
    }
}
