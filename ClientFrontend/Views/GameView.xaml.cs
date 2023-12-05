using System;
using System.Windows;
using System.Windows.Controls;

namespace ClientFrontend.Views;

public partial class GameView : Page
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