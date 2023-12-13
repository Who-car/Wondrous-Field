using System.Windows;
using System.Windows.Controls;

namespace ClientFrontend.Views
{
    public partial class WelcomeView : Page
    {
        private readonly Frame _mainFrame;
        public WelcomeView(Frame mainFrame)
        {
            _mainFrame = mainFrame;
            InitializeComponent();
        }

        private void StartGame(object sender, RoutedEventArgs e)
        {
            _mainFrame.Navigate(new CreateGameView(_mainFrame));
        }

        private void JoinGame(object sender, RoutedEventArgs e)
        {
            _mainFrame.Navigate(new JoinGameView(_mainFrame));
        }
    }
}