namespace nicesoon.Pages.AuthPages;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}

    private void ForgotPassword_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void LoginISVisiblePassword_button_Clicked(object sender, EventArgs e)
    {
        LoginPasswordEntry.IsPassword = !LoginPasswordEntry.IsPassword;
    }

    private void Register_label_Clicked(object sender, TappedEventArgs e)
    {
        Navigation.PushAsync(new RegistrationPage());
    }

    private void Login_button_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new MainPage());
    }

    private void OnPhoneChanhed2(object sender, TextChangedEventArgs e)
    {
        var entry = (Entry)sender;
        var newText = e.NewTextValue;

        if (string.IsNullOrEmpty(newText) || !newText.StartsWith("+7"))
        {
            entry.Text = "+7";
            return;
        }

        if (newText.Length > 2)
        {
            var numbers = new string(newText.Substring(2).Where(char.IsDigit).ToArray());

            if (numbers.Length > 10)
                numbers = numbers.Substring(0, 10);

            entry.Text = "+7" + numbers;

            entry.CursorPosition = entry.Text.Length;
        }
    }
}