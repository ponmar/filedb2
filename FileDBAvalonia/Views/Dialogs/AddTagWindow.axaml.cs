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

        public AddTagWindow(int? tagId = null)
        {
            InitializeComponent();
            DataContext = ServiceLocator.Resolve<AddTagViewModel>("tagId", tagId);
            this.RegisterForEvent<CloseModalDialogRequest>((x) => Close());
        }
    }
}
