using Avalonia.Controls;
using FileDBAvalonia.ViewModels;

namespace FileDBAvalonia.Views;

public partial class TagsView : UserControl
{
    public TagsView()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Resolve<TagsViewModel>();
    }
}
