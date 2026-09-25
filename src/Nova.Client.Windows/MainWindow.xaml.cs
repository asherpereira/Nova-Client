using System;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Nova.Client.Core.Auth;
using Nova.Client.Core.Networking;

namespace Nova.Client.Windows;

public sealed partial class MainWindow : Window
{
    private readonly AuthService _auth;
    private readonly NovaApiClient _api;
    private NovaRealtimeClient? _realtime;
    private Guid? _activeConversationId;
    private bool _serverRailExpanded;
    private bool _detailsExpanded;

    public MainWindow(AuthService auth, NovaApiClient api)
    {
        _auth = auth;
        _api = api;
        InitializeComponent();
        Closed += MainWindow_Closed;
        ApplyCurrentUser();
        _ = InitializeMessagingAsync();
    }

    private void ApplyCurrentUser()
    {
        var user = _auth.CurrentUser;
        if (user is null) return;

        var display = user.DisplayName ?? user.Username;
        ProfileDisplayNameText.Text = display;
        ProfileUsernameText.Text = "@" + user.Username;
        ProfileStatusText.Text = user.Status ?? "Online";
        ConversationTitleButton.Content = display;
    }

    private async Task InitializeMessagingAsync()
    {
        try
        {
            var conversations = await _api.ConversationsAsync();
            if (conversations is System.Text.Json.Nodes.JsonArray list)
            {
                var first = list
                    .OfType<System.Text.Json.Nodes.JsonObject>()
                    .FirstOrDefault();

                if (Guid.TryParse(first?["id"]?.GetValue<string>(), out var id))
                    _activeConversationId = id;
            }

            await ConnectRealtimeAsync();
        }
        catch
        {
            UtilityContextText.Text = "Offline";
        }
    }

    private async Task ConnectRealtimeAsync()
    {
        try
        {
            _realtime = new NovaRealtimeClient(_api.BaseUrl, () => _auth.CurrentSession?.AccessToken);
            _realtime.StateChanged += (_, state) =>
                DispatcherQueue.TryEnqueue(() => UtilityContextText.Text =
                    state == RealtimeState.Ready ? "Connected" : state.ToString());

            _realtime.EventReceived += (_, payload) =>
                DispatcherQueue.TryEnqueue(() => HandleRealtimeEvent(payload));

            await _realtime.ConnectAsync();
        }
        catch
        {
            UtilityContextText.Text = "Offline";
        }
    }

    private void HandleRealtimeEvent(System.Text.Json.Nodes.JsonObject payload)
    {
        if (string.Equals(payload["type"]?.GetValue<string>(), "ready", StringComparison.OrdinalIgnoreCase))
            return;

        if (string.Equals(payload["type"]?.GetValue<string>(), "message.created", StringComparison.OrdinalIgnoreCase))
        {
            var message = payload["message"]?.AsObject() ?? payload;
            var text = message["text"]?.GetValue<string>()
                ?? message["content"]?.GetValue<string>()
                ?? "[Encrypted message received]";
            AddMessage("Nova", text);
        }
    }

    private void ServerRail_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (_serverRailExpanded) return;
        _serverRailExpanded = true;
        ServerRailColumn.Width = new GridLength(176);
        UpdateServerRailLabels();
    }

    private void ServerRail_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (!_serverRailExpanded) return;
        _serverRailExpanded = false;
        ServerRailColumn.Width = new GridLength(62);
        UpdateServerRailLabels();
    }

    private void UpdateServerRailLabels()
    {
        foreach (var child in ServerList.Children)
        {
            if (child is Button button)
                button.HorizontalContentAlignment = _serverRailExpanded
                    ? HorizontalAlignment.Left
                    : HorizontalAlignment.Center;
        }
    }

    private void ProfileButton_Click(object sender, RoutedEventArgs e)
    {
        _detailsExpanded = !_detailsExpanded;
        DetailsColumn.Width = _detailsExpanded ? new GridLength(280) : new GridLength(0);
    }

    private void HomeButton_Click(object sender, RoutedEventArgs e) => UtilityContextText.Text = string.Empty;
    private void ServerButton_Click(object sender, RoutedEventArgs e) => UtilityContextText.Text = "Server";
    private void AddServerButton_Click(object sender, RoutedEventArgs e) => UtilityContextText.Text = "Add server";
    private void SettingsButton_Click(object sender, RoutedEventArgs e) => UtilityContextText.Text = "Settings";

    private void ConversationButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
            UtilityContextText.Text = button.Content?.ToString() ?? "Conversation";
    }

    private void AddGroupButton_Click(object sender, RoutedEventArgs e) => UtilityContextText.Text = "New DM group";
    private void CallButton_Click(object sender, RoutedEventArgs e) => UtilityContextText.Text = "Call";
    private void SearchButton_Click(object sender, RoutedEventArgs e) => UtilityContextText.Text = "Search";
    private void MoreButton_Click(object sender, RoutedEventArgs e) => UtilityContextText.Text = "Conversation menu";

    private void SendMessageButton_Click(object sender, RoutedEventArgs e) => _ = SendCurrentMessageAsync();

    private void MessageComposer_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == global::Windows.System.VirtualKey.Enter && !e.KeyStatus.WasKeyDown)
        {
            _ = SendCurrentMessageAsync();
            e.Handled = true;
        }
    }

    private async Task SendCurrentMessageAsync()
    {
        var text = MessageComposer.Text.Trim();
        if (string.IsNullOrWhiteSpace(text)) return;

        if (!_activeConversationId.HasValue)
        {
            UtilityContextText.Text = "No conversation selected";
            return;
        }

        // Nova's server contract accepts ciphertext only. Do not send plaintext
        // pretending it is encrypted; the audited E2EE layer must supply these
        // fields before this call is made.
        UtilityContextText.Text = "Encryption setup required";
    }

    private void AddMessage(string author, string text)
    {
        var textBrush = (Brush)Application.Current.Resources["NovaTextBrush"];
        var mutedBrush = (Brush)Application.Current.Resources["NovaMutedTextBrush"];

        var message = new StackPanel { Spacing = 2 };
        message.Children.Add(new TextBlock
        {
            Text = author,
            FontWeight = FontWeights.SemiBold,
            Foreground = textBrush
        });
        message.Children.Add(new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            Foreground = textBrush
        });
        message.Children.Add(new TextBlock
        {
            Text = DateTime.Now.ToString("HH:mm"),
            FontSize = 11,
            Foreground = mutedBrush
        });

        MessageList.Children.Add(message);
        MessageScrollViewer.ChangeView(null, MessageScrollViewer.ScrollableHeight, null);
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        _ = _realtime?.DisconnectAsync();
    }
}