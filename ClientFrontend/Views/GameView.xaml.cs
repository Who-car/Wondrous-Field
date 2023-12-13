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
    // TODO: посылать запросы на длину слова
    private const int WordLength = 7;
    private string _info;
    private ObservableCollection<CellContent> _copy;
    private AntpClient _client;
    // TODO: возможно можно избавиться от этих булек
    public bool HasChosen => !LetterChosen && !WordChosen && !IsTurn && !AnswerGiven;

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
        set => SetField(ref _isTurn, value);
    }

    public string Info
    {
        get => _info;
        set => SetField(ref _info, value);
    }

    private ObservableCollection<CellContent> WordLetters = new(
        new ObservableCollection<CellContent>(Enumerable.Repeat(new CellContent(), WordLength)));
    public ObservableCollection<Message> Messages = new();
    public GameView(Frame mainFrame, AntpClient client)
    {
        DataContext = this;
        InitializeComponent();
        
        _mainFrame = mainFrame;
        _client = client;

        _client.MessageReceived += message =>
            Messages.Add(new Message { Author = message.PlayerName, Text = message.Content });
        _client.OnTurn += info => IsTurn = client.IsTurn;
        _client.GameOver += winner => _mainFrame.Navigate(new VictoryView(_mainFrame, winner));
        CharactersControl.ItemsSource = WordLetters;
        MessagesControl.ItemsSource = Messages;
    }

    private void ChatInput_OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            ChatButton_Click(sender, e);
    }
    
    // TODO: Починить все async void
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
    
    // TODO: замена TextBox на TextBlock
    private async void TextBox_OnTextInput(object sender, RoutedEventArgs e)
    {
        if (LetterChosen)
        {
            var letter = (sender as TextBox)!.Text.ToCharArray()[0];
            if (await _client.CheckLetter(letter, 0))
            {
                Info = "Вы угадали! Партия довольна тобой, держи риску миса и кошко-жена";
            }
            else
            {
                (sender as TextBox)!.Text = "";
                Info = "Не угадали! Ход переходит к следующему игроку";
            }
        
            (sender as TextBox)!.IsEnabled = false;
            Keyboard.ClearFocus();
            AnswerGiven = true;
        }

        if (WordChosen)
        {
            if (WordLetters.All(item => !string.IsNullOrWhiteSpace(item.Text)))
            {
                var word = string.Join("", WordLetters.Select(c => c.Text));
                if (await _client.CheckWord(word))
                {
                    Info = "Какой вы молодец! Халифат горд!";
                }
                else
                {
                    Info = "Не получилось(";
                    WordLetters = new ObservableCollection<CellContent>(_copy);
                }
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
}