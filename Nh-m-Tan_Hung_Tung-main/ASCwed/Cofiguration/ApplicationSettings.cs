namespace ASCwed.Cofiguration
{
    public class ApplicationSettings
    {
        public string? ApplicationName { get; set; }
        public string? ApplicationTitle { get; set; }
        public string? AdminEmail { get; set; }
        public string? AdminName { get; set; }
        public string? AdminPassword { get; set; }
        public string? Roles { get; set; }
        public string? EngineerEmail { get; set; }
        public string? EngineerName { get; set; }
        public string? EngineerPassword { get; set; }
        public string? UserEmail { get; set; }
        public string? UserName { get; set; }
        public string? UserPassword { get; set; }

        public string? SMTPServer { get; set; }
        public int SMTPPort { get; set; }
        public string? SMTPAccount { get; set; }
        public string? SMTPPassword { get; set; }
    }
}
