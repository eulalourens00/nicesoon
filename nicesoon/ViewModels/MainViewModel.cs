using AndroidX.Navigation;
using nicesoon.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using nicesoon.Models;
using nicesoon.Services;

namespace nicesoon.ViewModels
{
    public class MainViewModel: BaseViewModel
    {
        //private bool _isBusy;
        //public bool isBusy
        //{
        //    get => _isBusy;
        //    set => SetProperty(ref _isBusy, value);
        //}

        //public ICommand LoginCommand { get; }
        //public ICommand GoToDiaryCommand { get; }

        //public MainViewModel()
        //{
        //    LoginCommand = new Command(async () => await LoginAsync());
        //    GoToDiaryCommand = new Command(async () => await GoToDiaryAsync());
        //}

        //private async Task LoginAsync()
        //{
        //    isBusy = true;
        //}

        //private async Task GoToDiaryAsync()
        //{
        //    await Shell.Current.GoToAsync("//diary");
        //}

        private User _currentUser;
        private bool _isLoggedIn;
        private string _welcomeMessage;

        // Свойства (связываются с View)
        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set
            {
                SetProperty(ref _isLoggedIn, value);
                OnPropertyChanged(nameof(WelcomeMessage)); // Обновим приветствие
            }
        }

        public string WelcomeMessage
        {
            get
            {
                if (IsLoggedIn && CurrentUser != null)
                    return $"Добро пожаловать, {CurrentUser.Username}!";
                return "Тихая гавань для ваших снов";
            }
        }

        // Команды
        public ICommand OpenDiaryCommand { get; }
        public ICommand OpenChatCommand { get; }
        public ICommand LogoutCommand { get; }

        // Сервисы
        private readonly ApiService _apiService;

        public MainViewModel(ApiService apiService)
        {
            _apiService = apiService;

            // Инициализация команд
            OpenDiaryCommand = new Command(async () => await OpenDiaryAsync());
            OpenChatCommand = new Command(async () => await OpenChatAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());

            // Загружаем состояние пользователя
            LoadUserState();
        }

        private async Task LoadUserState()
        {
            // Проверяем, есть ли сохраненный токен
            var token = await SecureStorage.GetAsync("auth_token");
            IsLoggedIn = !string.IsNullOrEmpty(token);

            if (IsLoggedIn)
            {
                // Загружаем данные пользователя
                CurrentUser = await _apiService.GetCurrentUserAsync();
            }
        }

        private async Task OpenDiaryAsync()
        {
            if (!IsLoggedIn)
            {
                await Shell.Current.GoToAsync("//login");
                return;
            }

            await Shell.Current.GoToAsync("//diary");
        }

        private async Task OpenChatAsync()
        {
            if (!IsLoggedIn)
            {
                await Shell.Current.GoToAsync("//login");
                return;
            }

            await Shell.Current.GoToAsync("//chat");
        }

        private async Task LogoutAsync()
        {
            // Очищаем токен
            SecureStorage.Remove("auth_token");
            IsLoggedIn = false;
            CurrentUser = null;

            // Возвращаем на главную
            await Shell.Current.GoToAsync("//main");
        }
    }
}
