using nicesoon.Models;
using nicesoon.Pages;
using nicesoon.Pages.AuthPages;
using nicesoon.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace nicesoon.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private User _currentUser;

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public bool IsLoggedIn => _authService.IsAuthenticated;

        public ICommand OpenDiaryCommand { get; }
        public ICommand OpenChatCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainViewModel(AuthService authService)
        {
            _authService = authService;

            _authService.AuthStateChanged += OnAuthStateChanged;

            LoadCurrentUser();

            OpenDiaryCommand = new Command(async () => await OpenDiaryAsync());
            OpenChatCommand = new Command(async () => await OpenChatAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());
        }

        private void OnAuthStateChanged(object sender, EventArgs e)
        {
            LoadCurrentUser();
            OnPropertyChanged(nameof(IsLoggedIn));
        }

        private void LoadCurrentUser()
        {
            CurrentUser = _authService.CurrentUser;
        }

        private async Task OpenDiaryAsync()
        {
            //if (!IsLoggedIn)
            //{
            //    await Application.Current.MainPage.Navigation.PushAsync(
            //    new LoginPage(ServiceLocator.LoginViewModel));
            //    return;
            //}

            await Application.Current.MainPage.Navigation.PushAsync(
                new NotesNightmares(ServiceLocator.DiaryViewModel));
        }

        private async Task OpenChatAsync()
        {
            await Application.Current.MainPage.Navigation.PushAsync(
                new ChatNicesoon(ServiceLocator.ChatViewModel));
        }

        private async Task LogoutAsync()
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Выход",
                "Вы уверены, что хотите выйти?",
                "Да", "Нет");

            if (confirm)
            {
                _authService.Logout();
                await Application.Current.MainPage.Navigation.PushAsync(
                new LoginPage(ServiceLocator.LoginViewModel));
            }
        }
        
    }
}
