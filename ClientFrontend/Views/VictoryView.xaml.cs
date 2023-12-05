using System;
using System.Windows;

namespace ClientFrontend.Views;

public partial class VictoryView : Window
{
    public VictoryView()
    {
        InitializeComponent();
    }

    private void StartNewGame(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("Someone wants to start new game again!");
    }
}