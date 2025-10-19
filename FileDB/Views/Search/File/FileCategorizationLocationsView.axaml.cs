using Avalonia.Controls;
using FileDB.ViewModels.Search.File;

namespace FileDB.Views.Search.File;

public partial class FileCategorizationLocationsView : UserControl
{
    public FileCategorizationLocationsView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<FileCategorizationViewModel>();
        }
    }
}