using Microsoft.UI.Xaml;
using Nova.Client.Core.Auth;
using Nova.Client.Core.Networking;

namespace Nova.Client.Windows;

public partial class App : Application
{
    public static Window? MainWindow { get; set; }
    public static AuthService? Auth { get; set; }
    public static NovaApiClient? Api { get; set; }

    public App() => InitializeComponent();

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        var apiUrl = Environment.GetEnvironmentVariable("NOVA_API_URL") ?? "http://localhost:8080";
        try
        {
            var api = new NovaApiClient(apiUrl);
            var auth = new AuthService(api, new SessionStore());

            if (await auth.RestoreAsync())
            {
                Auth = auth;
                Api = api;
                MainWindow = new MainWindow(auth, api);
                MainWindow.Activate();
                return;
            }
        }
        catch
        {
            // Fall through to the sign-in window. Startup should never expose raw errors.
        }

        var login = new LoginWindow();
        MainWindow = login;
        login.Activate();
    }
}