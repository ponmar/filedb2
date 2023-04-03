using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FileDB.Model;
using FileDB.ViewModel;

namespace FileDB.View;

/// <summary>
/// Interaction logic for FindView.xaml
/// </summary>
public partial class FindView : UserControl
{
    public FindView()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Resolve<FileInfoViewModel>();
    }
}
