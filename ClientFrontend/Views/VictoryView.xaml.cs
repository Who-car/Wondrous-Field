using System;
using System.Windows;
using System.Windows.Controls;

namespace ClientFrontend.Views;

public partial class VictoryView : Page
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