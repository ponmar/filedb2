using FileDBApp.View;

namespace FileDBApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(PersonDetailsPage), typeof(PersonDetailsPage));
        }
    }
}