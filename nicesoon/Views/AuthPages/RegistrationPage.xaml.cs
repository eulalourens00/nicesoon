namespace nicesoon.Pages;
using nicesoon.Pages.AuthPages;
using nicesoon.ViewModels;
public partial class RegistrationPage : ContentPage
{
	public RegistrationPage(RegistrationViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    
}