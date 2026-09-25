using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace Nova.Client.Windows;

public sealed partial class MainWindow : Window
{
    private bool _serverRailExpanded;
    private bool _detailsExpanded;

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
        // Keep the collapsed rail visual and the expanded rail useful without
        // coupling the navigation model to the eventual backend.
        if (ServerList is null)
            return;

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

    private void HomeButton_Click(object sender, RoutedEventArgs e)
    {
        UtilityContextText.Text = string.Empty;
    }

    private void ServerButton_Click(object sender, RoutedEventArgs e)
    {
        UtilityContextText.Text = "Server";
    }

    private void AddServerButton_Click(object sender, RoutedEventArgs e)
    {
        UtilityContextText.Text = "Add server";
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        UtilityContextText.Text = "Settings";
    }

    private void ConversationButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            var name = button.Content?.ToString() ?? "Conversation";
            UtilityContextText.Text = name;
        }
    }

    private void AddGroupButton_Click(object sender, RoutedEventArgs e)
    {
        UtilityContextText.Text = "New DM group";
    }

    private void CallButton_Click(object sender, RoutedEventArgs e)
    {
        UtilityContextText.Text = "Call";
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        UtilityContextText.Text = "Search";
    }

    private void MoreButton_Click(object sender, RoutedEventArgs e)
    {
        UtilityContextText.Text = "Conversation menu";
    }

    private void SendMessageButton_Click(object sender, RoutedEventArgs e)
    {
        SendCurrentMessage();
    }

    private void MessageComposer_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter &&
            !e.KeyStatus.WasKeyDown &&
            !IsShiftPressed())
        {
            SendCurrentMessage();
            e.Handled = true;
        }
    }

    private bool IsShiftPressed()
    {
        var state = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(
            Windows.System.VirtualKey.Shift);
        return state.HasFlag(Microsoft.UI.Input.KeyState.Down);
    }

    private void SendCurrentMessage()
    {
        var text = MessageComposer.Text.Trim();
        if (string.IsNullOrWhiteSpace(text))
            return;

        MessageList.Children.Add(new StackPanel
        {
            Spacing = 2,
            Children =
            {
                new TextBlock
                {
                    Text = "You",
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["NovaTextBrush"]
                },
                new TextBlock
                {
                    Text = text,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["NovaTextBrush"]
                },
                new TextBlock
                {
                    Text = DateTime.Now.ToString("HH:mm"),
                    FontSize = 11,
                    Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["NovaMutedTextBrush"]
                }
            }
        });

        MessageComposer.Text = string.Empty;
    }
}