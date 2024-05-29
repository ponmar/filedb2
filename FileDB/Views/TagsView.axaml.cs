using Avalonia.Controls;
using FileDB.ViewModels;

namespace FileDB.Views;

public partial class TagsView : UserControl
{
    public TagsView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<TagsViewModel>();
        }
    }
}
