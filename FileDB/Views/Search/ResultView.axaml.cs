using Avalonia.Controls;
using FileDB.Infrastructure;
using FileDB.ViewModels.Search;
using System.Diagnostics.CodeAnalysis;

namespace FileDB.Views.Search;

[ExcludeFromCodeCoverage]
public partial class ResultView : UserControl
{
    public ResultView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<ResultViewModel>();
        }
    }
}
