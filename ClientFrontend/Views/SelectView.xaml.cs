using System.Windows;
using System.Windows.Controls;
using Client;

namespace ClientFrontend.Views
{
    public partial class SelectView : Page
    {
        private readonly Frame _mainFrame;
        private readonly AntpClient _client;
        public SelectView(Frame mainFrame, AntpClient client)
        {
            _mainFrame = mainFrame;
            _client = client;
            InitializeComponent();
        }

        private void StartGame(object sender, RoutedEventArgs e)
        {
            _mainFrame.Navigate(new CreateGameView(_mainFrame, _client));
        }

        private void JoinGame(object sender, RoutedEventArgs e)
        {
            _mainFrame.Navigate(new JoinGameView(_mainFrame, _client));
        }
    }
}