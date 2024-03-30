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

        public AddPersonWindow(int? personId = null, string? personName = null)
        {
            InitializeComponent();
            var vm = ServiceLocator.Resolve<AddPersonViewModel>("personId", personId); ;
            DataContext = vm;
            if (personId is null && personName is not null)
            {
                if (personName.Contains(' '))
                {
                    var nameParts = personName.Split(' ');
                    vm.Firstname = nameParts[0];
                    if (nameParts.Length > 1)
                    {
                        vm.Lastname = nameParts[1];
                    }
                }
                else
                {
                    vm.Firstname = personName;
                }
            }
            this.RegisterForEvent<CloseModalDialogRequest>((x) => Close());
        }
    }
}
