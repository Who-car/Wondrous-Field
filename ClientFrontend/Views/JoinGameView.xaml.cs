using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClientFrontend.UIElementHelpers;

namespace ClientFrontend.Views;

public partial class JoinGameView : Page
{
    private readonly Frame _mainFrame;
    private readonly ObservableCollection<CellContent> SecretCode;
    public JoinGameView(Frame mainFrame)
    {
        InitializeComponent();
        DataContext = this;
        _mainFrame = mainFrame;
        SecretCode = new ObservableCollection<CellContent> { new(), new(), new(), new(), new() };
        CharactersControl.ItemsSource = SecretCode;
    }

    private void ReturnButton_Click(object sender, RoutedEventArgs e)
    {
        _mainFrame.GoBack();
    }
    
    private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
        var text = e.Text;

        var regex = new System.Text.RegularExpressions.Regex("^[0-9]*[a-z]*[A-Z]*$");
        e.Handled = !regex.IsMatch(text);
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (SecretCode.All(item => !string.IsNullOrWhiteSpace(item.Text)))
        {
            _mainFrame.Navigate(new GameView(_mainFrame));
        }
        else
        {
            if (sender is not TextBox tb || tb.Text.Length <= 0) return;
            var tRequest = new TraversalRequest(FocusNavigationDirection.Next);

            if (Keyboard.FocusedElement is UIElement keyboardFocus)
                keyboardFocus.MoveFocus(tRequest);
        }
    }

    private void PageLoaded(object sender, RoutedEventArgs e)
    {
        if (CharactersControl.ItemContainerGenerator.ContainerFromIndex(0) is not ContentPresenter container) return;
        var textBox = container.ContentTemplate.FindName("TextBox", container) as TextBox;
        textBox?.Focus();
    }
}