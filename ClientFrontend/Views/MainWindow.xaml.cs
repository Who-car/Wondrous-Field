using System.Windows;

namespace ClientFrontend.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MainFrame.Navigate(new WelcomeView(MainFrame));
    }
}