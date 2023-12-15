using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Client;
using ClientFrontend.UIElementHelpers;

namespace ClientFrontend.Views;

public partial class GameView : Page, INotifyPropertyChanged
{
    private readonly Frame _mainFrame;
    private bool _isLetter;
    private bool _isWord;
    private bool _isTurn;
    private bool _answerGiven;
    private string _info;
    private double _angle;
    private ObservableCollection<CellContent> _copy;
    private AntpClient _client;
    private string _question;
    public bool HasChosen => !LetterChosen && !WordChosen && IsTurn && !AnswerGiven;

    public bool LetterChosen
    {
        get => _isLetter;
        set
        {
            SetField(ref _isLetter, value);
            OnPropertyChanged(nameof(HasChosen));
        }
    }
    public bool WordChosen
    {
        get => _isWord;
        set
        {
            SetField(ref _isWord, value);
            OnPropertyChanged(nameof(HasChosen));
        }
    }
    public bool AnswerGiven
    {
        get => _answerGiven;
        set
        {
            SetField(ref _answerGiven, value);
            OnPropertyChanged(nameof(HasChosen));
        }
    }
    public bool IsTurn
    {
        get => _isTurn;
        set 
        {
            SetField(ref _isTurn, value);
            OnPropertyChanged(nameof(HasChosen));
        }
    }

    public string Info
    {
        get => _info;
        set => SetField(ref _info, value);
    }

    public double TargetAngle
    {
        get => _angle;
        set => SetField(ref _angle, value);
    }
    public string Question 
    { 
        get => _question; 
        set => SetField(ref _question, value); 
    }
    
    private ObservableCollection<CellContent> WordLetters;
    public ObservableCollection<Message> Messages = new();
    public GameView(Frame mainFrame, AntpClient client)
    {
        DataContext = this;
        InitializeComponent();
        
        _mainFrame = mainFrame;
        _client = client;
        var letterNum = client.SessionInfo.Word?.Length ?? 5;
        WordLetters = new ObservableCollection<CellContent>(Enumerable.Range(0, letterNum).Select(_ => new CellContent()));
        Question = client.SessionInfo.Riddle ?? "Default Riddle";

        _client.MessageReceived += message =>
            Application.Current.Dispatcher.Invoke(() => Messages.Add(new Message { Author = message.Player.Name, Text = message.Content }));
        
        _client.OnTurn += info => Application.Current.Dispatcher.Invoke(() =>
        {
            AnswerGiven = false;
            LetterChosen = false;
            WordChosen = false;

            IsTurn = client.IsTurn;
            for (var i = 0; i < WordLetters.Count; i++)
            {
                WordLetters[i].Text = info.Word![i].ToString();
            }

            Info = client.SessionInfo.IsGuessed
                ? "Угадал, халифат гордится тобой, держи риску миса и кошко-жена"
                : "Эй, не угадал, ход переходит следующему игроку";
        });
        _client.GameOver += winner => Application.Current.Dispatcher.Invoke(() => _mainFrame.Navigate(new VictoryView(_mainFrame, winner)));
        
        CharactersControl.ItemsSource = WordLetters;
        MessagesControl.ItemsSource = Messages;
    }

    private void ChatInput_OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            ChatButton_Click(sender, e);
    }
    
    private async void ChatButton_Click(object sender, RoutedEventArgs e)
    {
        await _client.ReportMessage(ChatInput.Text);
        Messages.Add(new Message()
        {
            Author = "Вы",
            Text = ChatInput.Text
        });
        ChatInput.Text = "";
    }
    
    private void TextBox_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        foreach (var cellContent in WordLetters)
        {
            cellContent.IsEnabled = false;
        } 
        
        (sender as TextBox)!.IsEnabled = true;
        Info = "Впишите букву";
    }
    
    private async void TextBox_OnTextInput(object sender, RoutedEventArgs e)
    {
        var text = (sender as TextBox)!.Text;
        if (string.IsNullOrEmpty(text))
            return;
        if (LetterChosen && !AnswerGiven)
        {
            var letter = text.ToCharArray()[0];
            AnswerGiven = true;
            // Task.Run(async() => await _client.ReportLetter(letter));
            await _client.ReportLetter(letter);
            // Application.Current.Dispatcher.Invoke(async () => await _client.ReportLetter(letter));
        
            (sender as TextBox)!.Text = "";
            (sender as TextBox)!.IsEnabled = false;
            Keyboard.ClearFocus();
        }

        if (WordChosen && !AnswerGiven)
        {
            if (WordLetters.All(item => !string.IsNullOrWhiteSpace(item.Text)))
            {
                var word = string.Join("", WordLetters.Select(c => c.Text));
                Task.Run(async () => await _client.ReportWord(word)).ConfigureAwait(false);
                AnswerGiven = true;
            }
            else
            {
                if (sender is not TextBox tb || tb.Text.Length <= 0) return;
                var tRequest = new TraversalRequest(FocusNavigationDirection.Next);

                if (Keyboard.FocusedElement is UIElement keyboardFocus)
                    keyboardFocus.MoveFocus(tRequest);
            }
        }
    }

    private void OpenLetter(object sender, RoutedEventArgs e)
    {
        LetterChosen = true;
        foreach (var cellContent in WordLetters)
        {
            cellContent.IsEnabled = true;
        }

        Info = "Выберите букву";
    }

    private void OpenWord(object sender, RoutedEventArgs e)
    {
        WordChosen = true;
        _copy = new ObservableCollection<CellContent>(WordLetters);
        foreach (var cellContent in WordLetters)
        {
            cellContent.IsEnabled = true;
        }
        Info = "Введите слово";
        
        if (CharactersControl.ItemContainerGenerator.ContainerFromIndex(0) is not ContentPresenter container) return;
        var textBox = container.ContentTemplate.FindName("TextBox", container) as TextBox;
        textBox?.Focus();
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(async () =>
        {
            var num = new Random().Next(0, 8);
            TargetAngle = 1080 + num * 45 - 22.5;
            RotateImage.Visibility = Visibility.Visible;
            await Task.Delay(5*1000);
            RotateImage.Visibility = Visibility.Collapsed;
        });
    }
}