using Avalonia.Controls;
using FileDBAvalonia.Model;
using FileDBAvalonia.ViewModels;

namespace FileDBAvalonia.Views.Dialogs
{
    public partial class AddTagWindow : Window
    {
        public AddTagWindow(int? tagId = null)
        {
            InitializeComponent();
            DataContext = ServiceLocator.Resolve<AddTagViewModel>("tagId", tagId);
            this.RegisterForEvent<CloseModalDialogRequest>((x) => Close());
        }
    }
}
