using ASC.Utilities.Models;

namespace ASCwed.Models.Navigation
{
    public class LeftNavigationViewModel
    {
        public CurrentUser CurrentUser { get; set; } = new();

        public IReadOnlyCollection<NavigationMenuItem> MenuItems { get; set; } = [];
    }
}
