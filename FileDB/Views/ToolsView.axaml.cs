using Avalonia.Controls;
using FileDB.ViewModels;

namespace FileDB.Views
{
    public partial class ToolsView : UserControl
    {
        public ToolsView()
        {
            InitializeComponent();
            if (!Design.IsDesignMode)
            {
                DataContext = ServiceLocator.Resolve<ToolsViewModel>();
            }
        }
    }
}
