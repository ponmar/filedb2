using FileDB.ViewModel;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace FileDB.View;

/// <summary>
/// Interaction logic for FileInfoMaximized.xaml
/// </summary>
public partial class FileInfoMaximized : UserControl
{
    public FileInfoMaximized()
    {
        InitializeComponent();
        DataContext = FindViewModel.Instance;
    }

    private void OpenLocationUri(object sender, RequestNavigateEventArgs e)
    {
        Utils.OpenUriInBrowser(e.Uri.AbsoluteUri);
        e.Handled = true;
    }
}
