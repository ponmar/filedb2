using Avalonia.Controls;
using FileDBAvalonia.Model;
using FileDBAvalonia.ViewModels;

namespace FileDBAvalonia.Views.Dialogs
{
    public partial class AddPersonWindow : Window
    {
        public AddPersonWindow(int? personId = null)
        {
            InitializeComponent();
            DataContext = ServiceLocator.Resolve<AddPersonViewModel>("personId", personId);
            this.RegisterForEvent<CloseModalDialogRequest>((x) => Close());
        }
    }
}
