using System.Windows.Controls;
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
