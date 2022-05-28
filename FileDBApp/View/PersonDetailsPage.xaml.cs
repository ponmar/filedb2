using FileDBApp.Model;
using FileDBApp.ViewModel;

namespace FileDBApp.View;

public partial class PersonDetailsPage : ContentPage
{
    public PersonDetailsPage(PersonDetailsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}