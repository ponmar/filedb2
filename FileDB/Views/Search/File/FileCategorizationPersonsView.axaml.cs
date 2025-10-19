using Avalonia.Controls;
using FileDB.ViewModels.Search.File;

namespace FileDB.Views.Search.File;

public partial class FileCategorizationPersonsView : UserControl
{
    public FileCategorizationPersonsView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<FileCategorizationViewModel>();
        }
    }
}