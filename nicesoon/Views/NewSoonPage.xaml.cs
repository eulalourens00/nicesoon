using nicesoon.ViewModels;

namespace nicesoon.Views;

public partial class NewSoonPage : ContentPage
{
	public NewSoonPage(EditNightmareViewModel viewmodel )
	{
		InitializeComponent();
		BindingContext = viewmodel;
	}
}