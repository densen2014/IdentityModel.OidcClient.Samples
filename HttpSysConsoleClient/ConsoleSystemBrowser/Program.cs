using IdentityModel.OidcClient;
using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleSystemBrowser;

class Program
{
    static string authority = "https://localhost:5001/";
    //static string authority = "https://ids2.app1.es/"; //真实环境
    static string api = $"{authority}WeatherForecast";
    static string clientId = "Blazor5002";

    static void Main(string[] args)
    {
        Console.WriteLine("+-----------------------+");
        Console.WriteLine("|  Sign in with OIDC    |");
        Console.WriteLine("+-----------------------+");
        Console.WriteLine("");

        Program p = new Program();
        p.Login();

        Console.ReadKey();
    }
    private async void Login()
    {

        string redirectUri = $"http://localhost/authentication/login-callback";
        string redirectLogoutUri = $"http://localhost/authentication/logout-callback";

        // create an HttpListener to listen for requests on that redirect URI.
        var http = new HttpListener();
        http.Prefixes.Add(redirectUri+"/");
        Console.WriteLine("Listening..");
        http.Start();

        var options = new OidcClientOptions
        {
            Authority = authority,
            ClientId = clientId,
            RedirectUri = redirectUri,
            PostLogoutRedirectUri = redirectLogoutUri,
            //Scope = "BlazorWasmIdentity.ServerAPI openid profile",
            Scope = "Densen.IdentityAPI openid profile",
            //Scope = "Blazor7.ServerAPI openid profile",
        };
            
        var client = new OidcClient(options);
        var state = await client.PrepareLoginAsync();

        Console.WriteLine($"Start URL: {state.StartUrl}");
        
        // open system browser to start authentication
        Process.Start(state.StartUrl);

        // wait for the authorization response.
        var context = await http.GetContextAsync();

        var formData = GetRequestPostData(context.Request);

        // Brings the Console to Focus.
        BringConsoleToFront();

        // sends an HTTP response to the browser.
        var response = context.Response;
        string responseString = $"<html><head><meta charset='utf-8' http-equiv='refresh' content='10;url={authority}'></head><body><h1>您现在可以返回应用程序.</h1></body></html>";
        var buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        var responseOutput = response.OutputStream;
        await responseOutput.WriteAsync(buffer, 0, buffer.Length);
        responseOutput.Close();

        Console.WriteLine($"Form Data: {formData}");
        var result = await client.ProcessResponseAsync(formData, state);

        if (result.IsError)
        {
            Console.WriteLine("\n\nError:\n{0}", result.Error);
        }
        else
        {
            Console.WriteLine("\n\nClaims:");
            foreach (var claim in result.User.Claims)
            {
                Console.WriteLine("{0}: {1}", claim.Type, claim.Value);
            }

            Console.WriteLine();
            Console.WriteLine("Access token:\n{0}", result.AccessToken);

            if (!string.IsNullOrWhiteSpace(result.RefreshToken))
            {
                Console.WriteLine("Refresh token:\n{0}", result.RefreshToken);
            }
        }

        http.Stop();
    }

    // Hack to bring the Console window to front.
    // ref: http://stackoverflow.com/a/12066376
    [DllImport("kernel32.dll", ExactSpelling = true)]
    public static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    public void BringConsoleToFront()
    {
        SetForegroundWindow(GetConsoleWindow());
    }

    public static string GetRequestPostData(HttpListenerRequest request)
    {
        //Get url code
        if (request.HttpMethod == "GET")
        {
            return request.Url.Query;
        }

        if (!request.HasEntityBody)
        {
            return null;
        }

        using (var body = request.InputStream)
        {
            using (var reader = new System.IO.StreamReader(body, request.ContentEncoding))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
