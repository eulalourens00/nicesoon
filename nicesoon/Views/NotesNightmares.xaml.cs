using nicesoon.ViewModels;

namespace nicesoon.Pages;

public partial class NotesNightmares : ContentPage
{
	public NotesNightmares(DiaryViewModel viewmodel)
	{
		InitializeComponent();
		BindingContext = viewmodel;
    }
}