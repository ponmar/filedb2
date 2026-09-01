using Avalonia.Controls;
using FileDB.Infrastructure;
using FileDB.ViewModels;
using System.Diagnostics.CodeAnalysis;

namespace FileDB.Views;

[ExcludeFromCodeCoverage]
public partial class BirthdaysView : UserControl
{
    public BirthdaysView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<BirthdaysViewModel>();
        }
    }
}
