using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nicesoon.Pages;
using nicesoon.Services;
using nicesoon.ViewModels;
namespace nicesoon.Services
{
    public static class ServiceLocator
    {
        private static AuthService _authService;
        private static ChatViewModel _chatViewModel;
        public static AuthService AuthService
        {
            get
            {
                if (_authService == null)
                    _authService = new AuthService();
                return _authService;
            }
        }

        public static MainViewModel MainViewModel => new MainViewModel(AuthService);
        public static LoginViewModel LoginViewModel => new LoginViewModel(AuthService);
        public static ChatViewModel ChatViewModel => new ChatViewModel();
        public static DiaryViewModel DiaryViewModel => new DiaryViewModel();

    }
}
