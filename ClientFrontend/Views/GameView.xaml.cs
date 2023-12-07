using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

    private readonly ObservableCollection<CellContent> WordLetters = new(new ObservableCollection<CellContent>(Enumerable.Repeat(new CellContent(), WordLength)));
    public GameView(Frame mainFrame)
    {
        DataContext = this;
        InitializeComponent();
        CharactersControl.ItemsSource = WordLetters;
        _mainFrame = mainFrame;
    }

    private void ChatButton_Click(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("Someone wants to chat!");
    }
    
    private void TextBox_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        (sender as TextBox)!.IsEnabled = true;
        LetterChosen = false;
    }
    
    private void TextBox_OnTextInput(object sender, RoutedEventArgs e)
    {
        (sender as TextBox)!.IsEnabled = false;
        //AnswerGiven = true;
    }

    private void OpenLetter(object sender, RoutedEventArgs e)
    {
        LetterChosen = true;
    }

    private void OpenWord(object sender, RoutedEventArgs e)
    {
        WordChosen = true;
        Console.WriteLine("Enter word");
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