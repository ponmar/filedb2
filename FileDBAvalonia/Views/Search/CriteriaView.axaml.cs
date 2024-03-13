using Avalonia.Controls;
using FileDBAvalonia.ViewModels;

namespace FileDBAvalonia.Views.Search;

public partial class CriteriaView : UserControl
{
    public CriteriaView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<CriteriaViewModel>();
        }
    }
}
