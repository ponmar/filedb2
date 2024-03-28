using System;
using Avalonia.Controls;
using FileDBAvalonia.Model;
using FileDBAvalonia.ViewModels.Dialogs;

namespace FileDBAvalonia.Views.Dialogs
{
    public partial class AddPersonWindow : Window
    {
        public AddPersonWindow()
        {
            InitializeComponent();
            if (!Design.IsDesignMode)
            {
                throw new NotSupportedException("The parameterless constructor shall only be used in design-mode");
            }
        }

        public AddPersonWindow(int? personId = null)
        {
            InitializeComponent();
            DataContext = ServiceLocator.Resolve<AddPersonViewModel>("personId", personId);
            this.RegisterForEvent<CloseModalDialogRequest>((x) => Close());
        }
    }
}
