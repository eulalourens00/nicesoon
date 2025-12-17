using nicesoon.Pages;
using nicesoon.Pages.AuthPages;

namespace nicesoon
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        private void Auth_ButtonClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new LoginPage());
        }

        private void OnDiary_Tapped(object sender, TappedEventArgs e)
        {
            Navigation.PushAsync(new NotesNightmares());
        }

        private void OnNicesoon_Tapped(object sender, TappedEventArgs e)
        {
            Navigation.PushAsync(new ChatNicesoon());
        }

       
    }

}
