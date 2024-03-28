using System;
using Avalonia.Controls;
using FileDBAvalonia.Model;
using FileDBAvalonia.ViewModels.Dialogs;

namespace FileDBAvalonia.Views.Dialogs
{
    public partial class AddLocationWindow : Window
    {
        public AddLocationWindow()
        {
            InitializeComponent();
            if (!Design.IsDesignMode)
            {
                throw new NotSupportedException("The parameterless constructor shall only be used in design-mode");
            }
        }

        public AddLocationWindow(int ?locationId = null)
        {
            InitializeComponent();
            DataContext = ServiceLocator.Resolve<AddLocationViewModel>("locationId", locationId);
            this.RegisterForEvent<CloseModalDialogRequest>((x) => Close());
        }
    }
}
