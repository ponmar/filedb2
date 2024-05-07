using System;
using Avalonia.Controls;
using FileDBAvalonia.Model;
using FileDBAvalonia.ViewModels.Dialogs;

namespace FileDBAvalonia.Views.Dialogs
{
    public partial class AddTagWindow : Window
    {
        public AddTagWindow()
        {
            InitializeComponent();
            if (!Design.IsDesignMode)
            {
                throw new NotSupportedException("The parameterless constructor shall only be used in design-mode");
            }
        }

        public AddTagWindow(int? tagId = null, string? tagName = null)
        {
            InitializeComponent();
            var vm = ServiceLocator.Resolve<AddTagViewModel>("tagId", tagId);
            DataContext = vm;
            if (tagId is null && tagName is not null)
            {
                vm.Name = tagName;
            }
            this.RegisterForEvent<CloseModalDialogRequest>((x) => Close());
        }
    }
}
