using Avalonia.Controls;
using FileDBAvalonia.ViewModels.Search;

namespace FileDBAvalonia.Views.Search;

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
