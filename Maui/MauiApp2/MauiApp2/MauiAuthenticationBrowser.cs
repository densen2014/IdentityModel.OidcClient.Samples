using IdentityModel.Client;
using IdentityModel.OidcClient.Browser;

namespace MauiApp2;

public class MauiAuthenticationBrowser : IdentityModel.OidcClient.Browser.IBrowser
{
    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri(options.StartUrl),
                new Uri(options.EndUrl));

#if WINDOWS
            var RedirectUri = "http://localhost/authentication/login-callback";
#else
            var RedirectUri = "myapp://callback/authentication/login-callback";
#endif
            var url = new RequestUrl(RedirectUri)
                      .Create(new Parameters(result.Properties));

            return new BrowserResult
            {
                Response = url,
                ResultType = BrowserResultType.Success
            };
        }
        catch (TaskCanceledException)
        {
            return new BrowserResult
            {
                ResultType = BrowserResultType.UserCancel
            };
        }
    }
}