using nicesoon.ViewModels;
namespace nicesoon.Pages;

public partial class ChatNicesoon : ContentPage
{

    public ChatNicesoon(ChatViewModel viewmodel)
    {
        InitializeComponent();
        BindingContext = viewmodel;
    }
}