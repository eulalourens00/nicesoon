using Microsoft.Extensions.Logging;
using nicesoon.Services;
using nicesoon.ViewModels;
using nicesoon.Pages.AuthPages;
using nicesoon.Pages;

namespace nicesoon
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // сервисы
            builder.Services.AddSingleton<LocalStorageService>();
            builder.Services.AddSingleton<ApiService>();

            // ViewModels
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<DiaryViewModel>();
            builder.Services.AddTransient<ChatViewModel>();

            // Views
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<NotesNightmares>();
            builder.Services.AddTransient<ChatNicesoon>();

            // маршруты
            Routing.RegisterRoute("//main", typeof(MainPage));
            Routing.RegisterRoute("//login", typeof(LoginPage));
            Routing.RegisterRoute("//diary", typeof(NotesNightmares));
            Routing.RegisterRoute("//chat", typeof(ChatNicesoon));

            return builder.Build();
        }
    }
}
