using Avalonia.Controls;
using FileDB.ViewModels;

namespace FileDB.Views.Search;

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
