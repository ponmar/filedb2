using Avalonia.Controls;
using FileDB.Infrastructure;
using FileDB.ViewModels;
using System.Diagnostics.CodeAnalysis;

namespace FileDB.Views.Search;

[ExcludeFromCodeCoverage]
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
