using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Client;

namespace ClientFrontend.Views;

public partial class WelcomeView : Page
{
    private Frame _mainFrame;
    private AntpClient _client;
    public WelcomeView(Frame frame)
    {
        InitializeComponent();
        _mainFrame = frame;
        _client = new AntpClient();
    }

    private void ChatInput_OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            ButtonBase_OnClick(sender, e);
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        // TODO: отображать сообщение об ошибке
        if (string.IsNullOrEmpty(NameInput.Text))
            return;

        _client.Player.Name = NameInput.Text;
        _mainFrame.Navigate(new SelectView(_mainFrame, _client));
    }

    private void WelcomeView_OnLoaded(object sender, RoutedEventArgs e)
    {
        NameInput.Focus();
    }
}