using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClientFrontend.UIElementHelpers;

namespace ClientFrontend.Views;

public partial class GameView : Page, INotifyPropertyChanged
{
    private readonly Frame _mainFrame;
    private bool _isLetter;
    private bool _isWord;
    private bool _isTurn;
    private bool _answerGiven;
    private const int WordLength = 7;
    private string _info;
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

    private readonly ObservableCollection<CellContent> WordLetters = new(
        new ObservableCollection<CellContent>(Enumerable.Repeat(new CellContent(), WordLength)));
    public ObservableCollection<Message> Messages = new();
    public GameView(Frame mainFrame)
    {
        DataContext = this;
        InitializeComponent();
        CharactersControl.ItemsSource = WordLetters;
        MessagesControl.ItemsSource = Messages;
        _mainFrame = mainFrame;
    }

    private void ChatInput_OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            ChatButton_Click(sender, e);
    }
    
    private void ChatButton_Click(object sender, RoutedEventArgs e)
    {
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
    private void TextBox_OnTextInput(object sender, RoutedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox.Text == "A")
        {
            Info = "Вы угадали! Ход переходит к следующему игроку";
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