using nicesoon.Models;
using nicesoon.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace nicesoon.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _phone = "+7";
        private string _password;
        private bool _isPasswordVisible;
        private bool _isLoading;
        private string _errorMessage;

        public string Phone
        {
            get => _phone;
            set
            {
                var formattedPhone = FormatPhoneNumber(value);
                SetProperty(ref _phone, formattedPhone);
                ValidateForm();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                ValidateForm();
            }
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoginEnabled => !IsLoading &&
                                     !string.IsNullOrWhiteSpace(Phone) &&
                                     Phone.Length >= 12 &&
                                     !string.IsNullOrWhiteSpace(Password) &&
                                     Password.Length >= 6;

        // Команды
        public ICommand LoginCommand { get; }
        public ICommand TogglePasswordCommand { get; }
        public ICommand ForgotPasswordCommand { get; }
        public ICommand RegisterCommand { get; }

        private readonly ApiService _apiService;

        private readonly AuthService _authService;
        public LoginViewModel(AuthService authService)
        {
            _authService = authService;

            LoginCommand = new Command(async () => await LoginAsync(), () => IsLoginEnabled);
            RegisterCommand = new Command(async () => await RegisterAsync());
            TogglePasswordCommand = new Command(() => IsPasswordVisible = !IsPasswordVisible);
            ForgotPasswordCommand = new Command(async () => await ForgotPasswordAsync());
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Phone) ||
                    e.PropertyName == nameof(Password) ||
                    e.PropertyName == nameof(IsLoading))
                {
                    (LoginCommand as Command)?.ChangeCanExecute();
                }
            };

            LoadLastPhone();
        }

        private async Task LoginAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                await SecureStorage.SetAsync("last_phone", Phone);

                var success = await _authService.LoginAsync(Phone, Password);

                if (success)
                {
                    await Application.Current.MainPage.Navigation.PushAsync(
                        new MainPage(ServiceLocator.MainViewModel));
                }
                else
                {
                    ErrorMessage = "Неверный номер телефона или пароль";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка входа: {ex.Message}";
                Console.WriteLine($"Login error: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void LoadLastPhone()
        {
            try
            {
                var lastPhone = await SecureStorage.GetAsync("last_phone");
                if (!string.IsNullOrEmpty(lastPhone))
                {
                    Phone = lastPhone;
                }
            }
            catch { }
        }

        private string FormatPhoneNumber(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "+7";

            var digits = new string(input.Where(c => char.IsDigit(c) || c == '+').ToArray());

            if (!digits.StartsWith("+7") && !digits.StartsWith("7"))
            {
                digits = "+7" + digits.TrimStart('+');
            }
            else if (digits.StartsWith("7"))
            {
                digits = "+" + digits;
            }

            if (digits.Length > 12)
            {
                digits = digits.Substring(0, 12);
            }

            if (digits.Length >= 12)
            {
                try
                {
                    return $"+7 ({digits.Substring(2, 3)}) {digits.Substring(5, 3)}-{digits.Substring(8, 2)}-{digits.Substring(10)}";
                }
                catch
                {
                    return digits;
                }
            }

            return digits;
        }

        private void ValidateForm()
        {
            ErrorMessage = string.Empty;

            if (!string.IsNullOrWhiteSpace(Phone) && Phone.Length < 12)
            {
                ErrorMessage = "Введите полный номер телефона";
            }
            else if (!string.IsNullOrWhiteSpace(Password) && Password.Length < 6)
            {
                ErrorMessage = "Пароль должен быть не менее 6 символов";
            }
        }

       
        private async Task ForgotPasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(Phone) || Phone.Length < 12)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Восстановление пароля",
                    "Введите номер телефона для восстановления доступа",
                    "OK");
                return;
            }

            await Application.Current.MainPage.DisplayAlert(
                "Восстановление пароля",
                "В демо-режиме пароль не требуется. Используйте любой пароль из 6+ символов.",
                "OK");
        }

        private async Task RegisterAsync()
        {
            await Shell.Current.GoToAsync("//register");
        }

        //nahui
        private string CleanPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return "+7";

            var digits = new string(phone.Where(char.IsDigit).ToArray());

            if (!digits.StartsWith("+7") && !digits.StartsWith("7"))
                digits = "+7" + digits.TrimStart('+');
            else if (digits.StartsWith("7"))
                digits = "+" + digits;

            if (digits.Length > 12)
                digits = digits.Substring(0, 12);

            if (digits.Length >= 12)
            {
                try
                {
                    return $"+7 ({digits.Substring(2, 3)}) {digits.Substring(5, 3)}-{digits.Substring(8, 2)}-{digits.Substring(10)}";
                }
                catch
                {
                    return digits;
                }
            }

            return digits;
        }
    }
}
