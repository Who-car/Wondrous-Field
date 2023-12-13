using System.Windows;
using Client;

namespace ClientFrontend.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MainFrame.Navigate(new WelcomeView(MainFrame));
    }
}