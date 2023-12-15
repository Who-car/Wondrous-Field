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
    public CreateGameView(Frame frame, AntpClient client)
    {
        InitializeComponent();
        
        _mainFrame = frame;
        _client = client;
        SecretCode = Task.Run(async () => 
           await GetSecretCode()).ConfigureAwait(false).GetAwaiter().GetResult();
        _client.OnGameStart += info => Application.Current.Dispatcher.Invoke(() => _mainFrame.Navigate(new GameView(_mainFrame, _client)));
        CharactersControl.ItemsSource = SecretCode;
    }

    private async Task<char[]> GetSecretCode()
    {
        var connection = await _client.StartNewGame().ConfigureAwait(false);
        
        if (connection.IsSuccessfulJoin)
            return connection.SessionId!.ToUpper().ToCharArray();
        
        MessageBox.Show($"Не получилось создать игру. Попробуйте позже");
        // TODO: Вызывающий поток не может получить доступ к данному объекту, так как владельцем этого объекта является другой поток.
        _mainFrame.GoBack();
        return Array.Empty<char>();
    }

    private void ReturnButton_Click(object sender, RoutedEventArgs e)
    {
        _mainFrame.GoBack();
    }
}