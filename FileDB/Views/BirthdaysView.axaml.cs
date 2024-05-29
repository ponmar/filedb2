using Avalonia.Controls;
using FileDB.ViewModels;

namespace FileDB.Views;

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
