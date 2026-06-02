using System;
using Avalonia.Controls;
using FileDB.Infrastructure;
using FileDB.Model;
using FileDB.ViewModels.Dialogs;

namespace FileDB.Views.Dialogs;

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
        var vm = ServiceLocator.Resolve<AddPersonViewModel>("personId", personId);
        DataContext = vm;
        if (personId is null && personName is not null)
        {
            vm.ShortName = personName;
            vm.FullName = personName;
        }
        this.RegisterForEvent<CloseModalDialogRequest>((x) => Close());
    }
}
