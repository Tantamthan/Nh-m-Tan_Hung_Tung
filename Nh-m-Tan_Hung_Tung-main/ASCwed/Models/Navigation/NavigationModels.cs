namespace ASCwed.Models.Navigation
{
    public class NavigationConfiguration
    {
        public List<NavigationMenuItem> MenuItems { get; set; } = [];
    }

    public class NavigationMenuItem
    {
        public string DisplayName { get; set; } = string.Empty;

        public string MaterialIcon { get; set; } = string.Empty;

        public string Link { get; set; } = string.Empty;

        public bool IsNested { get; set; }

        public int Sequence { get; set; }

        public List<string> UserRoles { get; set; } = [];

        public List<NavigationMenuItem> NestedItems { get; set; } = [];

        public bool IsAvailableFor(IEnumerable<string> roles)
        {
            if (UserRoles.Count == 0)
            {
                return true;
            }

            return roles.Any(role => UserRoles.Contains(role, StringComparer.OrdinalIgnoreCase));
        }

        public NavigationMenuItem FilterForRoles(IEnumerable<string> roles)
        {
            return new NavigationMenuItem
            {
                DisplayName = DisplayName,
                MaterialIcon = MaterialIcon,
                Link = Link,
                IsNested = IsNested,
                Sequence = Sequence,
                UserRoles = UserRoles,
                NestedItems = NestedItems
                    .Where(item => item.IsAvailableFor(roles))
                    .OrderBy(item => item.Sequence)
                    .Select(item => item.FilterForRoles(roles))
                    .ToList()
            };
        }
    }
}
