using Avalonia.Controls;
using FileDB.ViewModels.Search;

namespace FileDB.Views.Search;

public partial class FileView : UserControl
{
    public FileView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<FileViewModel>();
        }
    }
}
