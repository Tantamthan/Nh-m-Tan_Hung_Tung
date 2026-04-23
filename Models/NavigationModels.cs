namespace Lab8_PhamVanTung_2324801030079.Models;

public sealed class NavigationConfiguration
{
    public List<NavigationItem> Items { get; set; } = [];
}

public sealed class NavigationItem
{
    public string Title { get; set; } = string.Empty;

    public string? Area { get; set; }

    public string? Controller { get; set; }

    public string? Action { get; set; }

    public string? Page { get; set; }

    public string? Icon { get; set; }

    public string? ClientAction { get; set; }

    public List<string> UserRoles { get; set; } = [];

    public List<NavigationItem> Children { get; set; } = [];

    public bool HasChildren => Children.Count > 0;

    public bool IsVisibleTo(IReadOnlyCollection<string> roles)
    {
        return UserRoles.Count == 0
            || UserRoles.Intersect(roles, StringComparer.OrdinalIgnoreCase).Any();
    }

    public NavigationItem CloneWithChildren(IEnumerable<NavigationItem> children)
    {
        return new NavigationItem
        {
            Title = Title,
            Area = Area,
            Controller = Controller,
            Action = Action,
            Page = Page,
            Icon = Icon,
            ClientAction = ClientAction,
            UserRoles = [.. UserRoles],
            Children = [.. children]
        };
    }
}
