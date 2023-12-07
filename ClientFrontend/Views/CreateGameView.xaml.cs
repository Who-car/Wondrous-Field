using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClientFrontend.Views;

public partial class CreateGameView : Page
{
    private readonly Frame _mainFrame;
    private readonly char[] SecretCode;
    public CreateGameView(Frame frame)
    {
        InitializeComponent();
        _mainFrame = frame;
        SecretCode = "cat".ToUpper().Take(5).ToArray();
        CharactersControl.ItemsSource = SecretCode;
    }

    private void ReturnButton_Click(object sender, RoutedEventArgs e)
    {
        _mainFrame.GoBack();
    }
}