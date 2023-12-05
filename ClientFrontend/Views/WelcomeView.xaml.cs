using System;
using System.Windows;

namespace ClientFrontend
{
    public partial class WelcomeView : Window
    {
        public WelcomeView()
        {
            InitializeComponent();
        }

        private void StartGame(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Someone wants to start new game!");
        }

        private void JoinGame(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Someone wants to join game!");
        }
    }
}