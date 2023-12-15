using System;
using System.Windows;
using System.Windows.Controls;
using ClientServerTransfer;

namespace ClientFrontend.Views;

public partial class VictoryView : Page
{
    private Frame _mainFrame;
    public string WinnerText { get; set; }
    public VictoryView(Frame mainFrame, Player winner)
    {
        InitializeComponent();
        DataContext = this;

        _mainFrame = mainFrame;
        WinnerText = $"{winner.Name.ToUpper()} ВЫИГРАЛ\nОчки: {winner.Points}";
    }

    private void StartNewGame(object sender, RoutedEventArgs e)
    {
        _mainFrame.Navigate(new WelcomeView(_mainFrame));
    }
}