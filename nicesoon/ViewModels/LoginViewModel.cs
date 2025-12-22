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

        // Свойства для связывания с View
        public string Phone
        {
            get => _phone;
            set
            {
                // Форматируем номер телефона
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
                                     Phone.Length >= 12 && // +7XXXYYYYYYY
                                     !string.IsNullOrWhiteSpace(Password) &&
                                     Password.Length >= 6;

        // Команды
        public ICommand LoginCommand { get; }
        public ICommand TogglePasswordCommand { get; }
        public ICommand ForgotPasswordCommand { get; }
        public ICommand RegisterCommand { get; }

        private readonly LocalStorageService _localStorage;
        private readonly ApiService _apiService;

        public LoginViewModel(LocalStorageService localStorage, ApiService apiService)
        {
            _localStorage = localStorage;
            _apiService = apiService;

            LoginCommand = new Command(async () => await LoginAsync(), () => IsLoginEnabled);
            TogglePasswordCommand = new Command(() => IsPasswordVisible = !IsPasswordVisible);
            ForgotPasswordCommand = new Command(async () => await ForgotPasswordAsync());
            RegisterCommand = new Command(async () => await RegisterAsync());

            // Обновляем доступность команды при изменении свойств
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Phone) ||
                    e.PropertyName == nameof(Password) ||
                    e.PropertyName == nameof(IsLoading))
                {
                    (LoginCommand as Command)?.ChangeCanExecute();
                }
            };

            // Загружаем последний использованный телефон
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
            catch
            {
                // Игнорируем ошибки
            }
        }

        private string FormatPhoneNumber(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "+7";

            // Удаляем все нецифровые символы, кроме +
            var digits = new string(input.Where(c => char.IsDigit(c) || c == '+').ToArray());

            // Если нет +7 в начале, добавляем
            if (!digits.StartsWith("+7") && !digits.StartsWith("7"))
            {
                digits = "+7" + digits.TrimStart('+');
            }
            else if (digits.StartsWith("7"))
            {
                digits = "+" + digits;
            }

            // Ограничиваем длину (код страны + 10 цифр)
            if (digits.Length > 12)
            {
                digits = digits.Substring(0, 12);
            }

            // Форматируем: +7 (XXX) XXX-XX-XX
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

        private async Task LoginAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                // Сохраняем телефон для будущих сессий
                await SecureStorage.SetAsync("last_phone", Phone);

                // Очищаем номер от форматирования для проверки
                var cleanPhone = new string(Phone.Where(char.IsDigit).ToArray());

                // В реальном приложении здесь была бы проверка с сервером
                // Для демо: проверяем в локальной БД
                var existingUser = await _localStorage.GetUserByPhoneAsync(cleanPhone);

                if (existingUser != null)
                {
                    // В реальном приложении проверяем хэш пароля!
                    // Для демо: любой пароль подойдет

                    // Обновляем время последнего входа
                    existingUser.LastLogin = DateTime.Now;
                    await _localStorage.SaveUserAsync(existingUser);

                    // Сохраняем ID пользователя в SecureStorage
                    await SecureStorage.SetAsync("current_user_id", existingUser.Id.ToString());

                    // Если у пользователя нет записей, создаем демо-данные
                    var records = await _localStorage.GetRecordsAsync(existingUser.Id);
                    if (!records.Any())
                    {
                        await _localStorage.CreateDemoDataAsync(existingUser.Id);
                    }

                    // Переходим на главную страницу
                    await Shell.Current.GoToAsync("//main");
                }
                else
                {
                    // Пользователь не найден - создаем нового (упрощенная регистрация)
                    var newUser = new User
                    {
                        Phone = cleanPhone,
                        Username = $"Пользователь {cleanPhone.Substring(cleanPhone.Length - 4)}",
                        PasswordHash = "demo_hash", // В реальном приложении хэшируем!
                        CreatedAt = DateTime.Now,
                        LastLogin = DateTime.Now
                    };

                    await _localStorage.SaveUserAsync(newUser);

                    // Создаем демо-данные
                    await _localStorage.CreateDemoDataAsync(newUser.Id);

                    // Сохраняем ID
                    await SecureStorage.SetAsync("current_user_id", newUser.Id.ToString());

                    // Переходим на главную
                    await Shell.Current.GoToAsync("//main");
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

            // В реальном приложении здесь отправка SMS/email
            await Application.Current.MainPage.DisplayAlert(
                "Восстановление пароля",
                "В демо-режиме пароль не требуется. Используйте любой пароль из 6+ символов.",
                "OK");
        }

        private async Task RegisterAsync()
        {
            // Переход на страницу регистрации
            await Shell.Current.GoToAsync("//register");
        }
    }
}
