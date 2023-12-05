using System.Windows;
using System.Windows.Controls;

namespace ClientFrontend.Views;

public partial class CreateGameView : Page
{
    private readonly Frame _mainFrame;
    public CreateGameView(Frame frame)
    {
        _mainFrame = frame;
        InitializeComponent();
    }

    private void ReturnButton_Click(object sender, RoutedEventArgs e)
    {
        _mainFrame.GoBack();
    }
}