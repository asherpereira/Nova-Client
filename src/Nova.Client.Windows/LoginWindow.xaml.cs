using Microsoft.UI.Xaml;
using Nova.Client.Core.Auth;
using Nova.Client.Core.Networking;

namespace Nova.Client.Windows;

public sealed partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        await AuthenticateAsync(register: false);
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        await AuthenticateAsync(register: true);
    }

    private async Task AuthenticateAsync(bool register)
    {
        var username = UsernameBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            SetStatus("Enter a username and password.");
            return;
        }

        LoginButton.IsEnabled = false;
        RegisterButton.IsEnabled = false;
        SetStatus(register ? "Creating your account…" : "Signing in…");

        try
        {
            var apiUrl = ApiUrlBox.Text.Trim();
            var api = new NovaApiClient(apiUrl);
            var auth = new AuthService(api, new SessionStore());

            if (register)
            {
                var displayName = username;
                await auth.RegisterAsync(username, displayName, password);
            }
            else
            {
                await auth.LoginAsync(username, password);
            }

            App.Auth = auth;
            App.Api = api;

            var main = new MainWindow(auth, api);
            App.MainWindow = main;
            main.Activate();
            Close();
        }
        catch (NovaApiException ex)
        {
            SetStatus(ex.Message);
        }
        catch (HttpRequestException)
        {
            SetStatus("Unable to reach the Nova server. Check the backend URL.");
        }
        catch (TaskCanceledException)
        {
            SetStatus("The request timed out.");
        }
        catch (Exception ex)
        {
            SetStatus($"Sign-in failed: {ex.Message}");
        }
        finally
        {
            LoginButton.IsEnabled = true;
            RegisterButton.IsEnabled = true;
        }
    }

    private void SetStatus(string message) => StatusText.Text = message;
}