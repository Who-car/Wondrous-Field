using System;
using System.Windows;

namespace ClientFrontend.Views;

public partial class GameView : Window
{
    public GameView()
    {
        InitializeComponent();
    }

    private void ChatButton_Click(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("Someone wants to chat!");
    }

    private void OpenLetter(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("Enter any letter");
    }

    private void OpenWord(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("Enter word");
    }
}