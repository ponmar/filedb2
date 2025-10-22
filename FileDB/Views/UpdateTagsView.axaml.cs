using Avalonia.Controls;
using FileDB.ViewModels;

namespace FileDB.Views;

public partial class UpdateTagsView : UserControl
{
    public UpdateTagsView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<UpdateTagsViewModel>();
        }
    }
}
