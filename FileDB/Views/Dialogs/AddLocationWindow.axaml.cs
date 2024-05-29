using System;
using Avalonia.Controls;
using FileDB.Model;
using FileDB.ViewModels.Dialogs;

namespace FileDB.Views.Dialogs
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

        public AddLocationWindow(int ?locationId = null, string? locationName = null)
        {
            InitializeComponent();
            var vm = ServiceLocator.Resolve<AddLocationViewModel>("locationId", locationId);
            DataContext = vm;
            if (locationId is null && locationName is not null)
            {
                vm.Name = locationName;
            }
            this.RegisterForEvent<CloseModalDialogRequest>((x) => Close());
        }
    }
}
