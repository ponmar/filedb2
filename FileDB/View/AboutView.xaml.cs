using System.Windows.Controls;
using System.Windows.Navigation;
using FileDB.ViewModel;

namespace FileDB.View;

/// <summary>
/// Interaction logic for ToolsView.xaml
/// </summary>
public partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();
        DataContext = new AboutViewModel();
    }

    private void OpenUri(object sender, RequestNavigateEventArgs e)
    {
        Utils.OpenUriInBrowser(e.Uri.AbsoluteUri);
        e.Handled = true;
    }
}
