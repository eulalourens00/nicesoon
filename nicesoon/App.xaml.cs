using nicesoon.Pages.AuthPages;
using nicesoon.Services;
using nicesoon.Pages;
using nicesoon.Views;
namespace nicesoon
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            try
            {
                var mainPage = new MainPage(ServiceLocator.MainViewModel);
                MainPage = new NavigationPage(mainPage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания MainPage: {ex.Message}");

                MainPage = new Test();
            }
        }
    }
}
