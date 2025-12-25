using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using nicesoon.Services;
namespace nicesoon.ViewModels
{
    public class RegistrationViewModel : BaseViewModel
    {
        private string _name;
        private string _phone = "+7";
        private string _password;
        private string _confirmPassword;
        private bool _isPasswordVisible;
        private bool _isConfirmPasswordVisible;
        private bool _isLoading;
        private string _errorMessage;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

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

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                SetProperty(ref _confirmPassword, value);
                ValidateForm();
            }
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }

        public bool IsConfirmPasswordVisible
        {
            get => _isConfirmPasswordVisible;
            set => SetProperty(ref _isConfirmPasswordVisible, value);
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

        public bool IsRegisterEnabled => !IsLoading &&
                                        !string.IsNullOrWhiteSpace(Name) &&
                                        Name.Length >= 2 &&
                                        !string.IsNullOrWhiteSpace(Phone) &&
                                        Phone.Length >= 12 &&
                                        !string.IsNullOrWhiteSpace(Password) &&
                                        Password.Length >= 6 &&
                                        Password == ConfirmPassword;

        public ICommand RegisterCommand { get; }
        public ICommand TogglePasswordCommand { get; }
        public ICommand ToggleConfirmPasswordCommand { get; }
        public ICommand LoginCommand { get; }

        private readonly AuthService _authService;

        public RegistrationViewModel(AuthService authService)
        {
            _authService = authService;

            RegisterCommand = new Command(async () => await RegisterAsync(), () => IsRegisterEnabled);
            TogglePasswordCommand = new Command(() => IsPasswordVisible = !IsPasswordVisible);
            ToggleConfirmPasswordCommand = new Command(() => IsConfirmPasswordVisible = !IsConfirmPasswordVisible);
            LoginCommand = new Command(async () => await LoginAsync());

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Name) ||
                    e.PropertyName == nameof(Phone) ||
                    e.PropertyName == nameof(Password) ||
                    e.PropertyName == nameof(ConfirmPassword) ||
                    e.PropertyName == nameof(IsLoading))
                {
                    (RegisterCommand as Command)?.ChangeCanExecute();
                }
            };

            LoadLastPhone();
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
            if (string.IsNullOrEmpty(input)) return "+7";

            var digits = new string(input.Where(c => char.IsDigit(c) || c == '+').ToArray());

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

        private void ValidateForm()
        {
            ErrorMessage = string.Empty;

            if (!string.IsNullOrWhiteSpace(Name) && Name.Length < 2)
                ErrorMessage = "Имя должно быть не короче 2 символов";
            else if (!string.IsNullOrWhiteSpace(Phone) && Phone.Length < 12)
                ErrorMessage = "Введите полный номер телефона";
            else if (!string.IsNullOrWhiteSpace(Password) && Password.Length < 6)
                ErrorMessage = "Пароль должен быть не менее 6 символов";
            else if (!string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                     Password != ConfirmPassword)
                ErrorMessage = "Пароли не совпадают";
        }

        private async Task RegisterAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                await SecureStorage.SetAsync("last_phone", Phone);

                var success = await _authService.RegisterAsync(Phone, Name, Password);

                if (success)
                {
                    await Shell.Current.DisplayAlert("Успех", "Регистрация завершена!", "OK");
                    await Shell.Current.GoToAsync("//main");
                }
                else
                {
                    ErrorMessage = "Пользователь с таким номером уже существует";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка регистрации: {ex.Message}";
                Console.WriteLine($"Registration error: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoginAsync()
        {
            await Shell.Current.GoToAsync("//login");
        }
    }
}
