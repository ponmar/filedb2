using Avalonia.Controls;
using FileDBAvalonia.Model;
using FileDBAvalonia.ViewModels;

namespace FileDBAvalonia.Views.Dialogs
{
    public partial class AddLocationWindow : Window
    {
        public AddLocationWindow(int ?locationId = null)
        {
            InitializeComponent();
            DataContext = ServiceLocator.Resolve<AddLocationViewModel>("locationId", locationId);
            this.RegisterForEvent<CloseModalDialogRequest>((x) => Close());
        }
    }
}
