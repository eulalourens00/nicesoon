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

            // services
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<ApiService>();

            // viewmodels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegistrationViewModel>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<DiaryViewModel>();
            builder.Services.AddTransient<ChatViewModel>();

            // views
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegistrationPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<NotesNightmares>();
            builder.Services.AddTransient<ChatNicesoon>();

            return builder.Build();
        }
    }
}
