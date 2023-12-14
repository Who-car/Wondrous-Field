using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Client;

namespace ClientFrontend.Views;

public partial class CreateGameView : Page
{
    private readonly Frame _mainFrame;
    private readonly char[] SecretCode;
    private readonly AntpClient _client;
    public CreateGameView(Frame frame)
    {
        InitializeComponent();
        
        // TODO: как работает GetAwaiter???
        _mainFrame = frame;
        _client = new AntpClient();
        SecretCode = GetSecretCode().GetAwaiter().GetResult();

        _client.OnGameStart += info => _mainFrame.Navigate(new GameView(_mainFrame, _client));
        CharactersControl.ItemsSource = SecretCode;
    }

    private async Task<char[]> GetSecretCode()
    {
        var connection = await Task.Run(async () => await _client.StartNewGame().ConfigureAwait(false)).ConfigureAwait(false);
        
        if (connection.IsSuccessfulJoin)
            return connection.SessionId!.ToUpper().ToCharArray();
        
        MessageBox.Show($"Не получилось создать игру. Попробуйте позже");
        _mainFrame.GoBack();
        return Array.Empty<char>();
    }

    private void ReturnButton_Click(object sender, RoutedEventArgs e)
    {
        _mainFrame.GoBack();
    }
}