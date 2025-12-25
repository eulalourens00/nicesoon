using nicesoon.Pages;
using nicesoon.Pages.AuthPages;
using nicesoon.ViewModels;
using nicesoon.Services;

namespace nicesoon
{
    public partial class MainPage : ContentPage
    {

        public MainPage(MainViewModel viewmodel)
        {
            InitializeComponent();
            BindingContext = ServiceLocator.MainViewModel;
        }       
    }

}
