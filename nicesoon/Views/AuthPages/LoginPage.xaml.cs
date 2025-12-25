using nicesoon.ViewModels;
using nicesoon.Services;
namespace nicesoon.Pages.AuthPages;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = ServiceLocator.LoginViewModel;
    }
    
}