using Avalonia.Controls;
using FileDBAvalonia.ViewModels.Search;

namespace FileDBAvalonia.Views.Search;

public partial class Result : UserControl
{
    public Result()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Resolve<ResultViewModel>();
    }
}
