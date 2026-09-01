using Avalonia.Controls;
using FileDB.Infrastructure;
using FileDB.ViewModels.Search.File;
using System.Diagnostics.CodeAnalysis;

namespace FileDB.Views.Search.File;

[ExcludeFromCodeCoverage]
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
