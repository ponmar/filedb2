using FileDBApp.ViewModel;

namespace FileDBApp.View;

public partial class RipPage : ContentPage
{
	public RipPage(RipViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}