using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Media;

namespace Nova.Client.Windows;

public sealed partial class MainWindow : Window
{
    private bool _serverRailExpanded;
    private bool _detailsExpanded;
    private readonly AuthService _auth;
    private readonly NovaApiClient _api;
    private NovaRealtimeClient? _realtime;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void ServerRail_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (_serverRailExpanded)
            return;

        _serverRailExpanded = true;
        ServerRailColumn.Width = new GridLength(176);
        UpdateServerRailLabels();
    }

    private void ServerRail_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (!_serverRailExpanded)
            return;

        _serverRailExpanded = false;
        ServerRailColumn.Width = new GridLength(62);
        UpdateServerRailLabels();
    }

    private void UpdateServerRailLabels()
    {
        foreach (var child in ServerList.Children)
        {
            if (child is Button button)
            {
                button.HorizontalContentAlignment = _serverRailExpanded
                    ? HorizontalAlignment.Left
                    : HorizontalAlignment.Center;
            }
        }
    }

    private void ProfileButton_Click(object sender, RoutedEventArgs e)
    {
        _detailsExpanded = !_detailsExpanded;
        DetailsColumn.Width = _detailsExpanded
            ? new GridLength(280)
            : new GridLength(0);
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

    private void SendMessageButton_Click(object sender, RoutedEventArgs e) => SendCurrentMessage();

    private void MessageComposer_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == global::Windows.System.VirtualKey.Enter && !e.KeyStatus.WasKeyDown)
        {
            SendCurrentMessage();
            e.Handled = true;
        }
    }

    private void SendCurrentMessage()
    {
        var text = MessageComposer.Text.Trim();
        if (string.IsNullOrWhiteSpace(text))
            return;

        AddMessage("You", text);\n    }\n\n    private void AddMessage(string author, string text)\n    {\n        var textBrush = (Brush)Application.Current.Resources["NovaTextBrush"];
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
        MessageComposer.Text = string.Empty;
    }
}