using FileDBApp.Services;
using FileDBApp.View;
using FileDBApp.ViewModel;

namespace FileDBApp
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

            builder.Services.AddSingleton<PersonService>();

            builder.Services.AddSingleton<BirthdaysViewModel>();
            builder.Services.AddSingleton<RipViewModel>();

            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<RipPage>();

            return builder.Build();
        }
    }
}