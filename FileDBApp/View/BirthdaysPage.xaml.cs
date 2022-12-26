using FileDBApp.ViewModel;

namespace FileDBApp.View;

public partial class BirthdaysPage : ContentPage
{
    public BirthdaysPage(BirthdaysViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}