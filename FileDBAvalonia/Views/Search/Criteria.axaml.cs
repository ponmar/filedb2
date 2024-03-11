using Avalonia.Controls;
using FileDBAvalonia.ViewModels;

namespace FileDBAvalonia.Views.Search;

public partial class Criteria : UserControl
{
    public Criteria()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Resolve<CriteriaViewModel>();
    }
}
