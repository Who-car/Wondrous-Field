using System.Windows;
using System.Windows.Controls;

namespace ClientFrontend.Views;

public partial class JoinGameView : Page
{
    private readonly Frame _mainFrame;
    public JoinGameView(Frame mainFrame)
    {
        _mainFrame = mainFrame;
        InitializeComponent();
    }

    private void ReturnButton_Click(object sender, RoutedEventArgs e)
    {
        _mainFrame.GoBack();
    }
}