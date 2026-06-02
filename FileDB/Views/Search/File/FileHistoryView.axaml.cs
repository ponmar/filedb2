using Avalonia.Controls;
using FileDB.Infrastructure;
using FileDB.ViewModels.Search.File;

namespace FileDB.Views.Search.File;

public partial class FileHistoryView : UserControl
{
    public FileHistoryView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<FileCategorizationViewModel>();
        }
    }
}
