using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using Microsoft.Maui.ApplicationModel;

namespace MauiApp2;

public static class MauiProgram
{
    //static string authority = "https://localhost:5001/";
    static string authority = "https://ids2.app1.es/"; //真实环境
    static string api = $"{authority}WeatherForecast";
    static string clientId = "Blazor5002";
#if WINDOWS
    static string redirectUri = "http://localhost/authentication/login-callback";
    static string redirectLogoutUri = "http://localhost/authentication/logout-callback";
#else
    static string redirectUri = "myapp://callback/authentication/login-callback";
    static string redirectLogoutUri = "myapp://localhost/authentication/logout-callback";
#endif

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

        // add main page
        builder.Services.AddSingleton<MainPage>();


        // setup OidcClient
        builder.Services.AddSingleton(new OidcClient(new()
        {
            Authority = authority,

            ClientId = clientId,
            Scope = "BlazorWasmIdentity.ServerAPI openid profile",
            RedirectUri = redirectUri,
            PostLogoutRedirectUri = redirectLogoutUri,
            Browser = new MauiAuthenticationBrowser()
        }));

        return builder.Build();
    }
}