using FileDBApp.ViewModel;

namespace FileDBApp
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