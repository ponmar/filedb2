using Avalonia.Controls;
using FileDBAvalonia.ViewModels.Search;

namespace FileDBAvalonia.Views.Search;

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
