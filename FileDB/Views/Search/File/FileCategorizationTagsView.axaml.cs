using Avalonia.Controls;
using FileDB.ViewModels.Search.File;

namespace FileDB.Views.Search.File;

public partial class FileCategorizationTagsView : UserControl
{
    public FileCategorizationTagsView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<FileCategorizationViewModel>();
        }
    }
}