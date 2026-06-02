using Avalonia.Controls;
using FileDB.Infrastructure;
using FileDB.ViewModels.Search;

namespace FileDB.Views.Search;

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
