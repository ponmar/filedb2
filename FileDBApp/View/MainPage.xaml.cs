using FileDBApp.ViewModel;

namespace FileDBApp.View
{
    public partial class MainPage : ContentPage
    {
        public MainPage(BirthdaysViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}