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
    private bool _wheel;
    private string _info;
    private double _angle;
    private ObservableCollection<CellContent> _copy;
    private AntpClient _client;
    private string _question;
    private int _potentialScore;
    private static readonly int[] scores = new int[] { 500, 100, 300, 600, -100, 200, 400, -200 };
    public bool HasChosen => !LetterChosen && !WordChosen && IsTurn && !_answerGiven && _wheel;

    public bool LetterChosen
    {
        get => _isLetter && _wheel && !_answerGiven;
        set
        {
            SetField(ref _isLetter, value);
            OnPropertyChanged(nameof(HasChosen));
            OnPropertyChanged(nameof(AnswerGiven));
        }
    }
    public bool WordChosen
    {
        get => _isWord && _wheel && !_answerGiven;
        set
        {
            SetField(ref _isWord, value);
            OnPropertyChanged(nameof(HasChosen));
            OnPropertyChanged(nameof(AnswerGiven));
        }
    }
    public bool AnswerGiven
    {
        get => !_answerGiven && (WordChosen || LetterChosen);
        set
        {
            SetField(ref _answerGiven, !value);
            OnPropertyChanged(nameof(HasChosen));
        }
    }
    public bool WheelSpinned
    {
        get => !_wheel && _isTurn;
        set
        {
            SetField(ref _wheel, !value);
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
            OnPropertyChanged(nameof(WheelSpinned));
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
    public int PotentialScore
    {
        get => _potentialScore; 
        set => SetField(ref _potentialScore, value);
    }
    
    public ObservableCollection<CellContent> WordLetters;
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
            LetterChosen = false;
            WordChosen = false;
            IsTurn = client.IsTurn;
            WheelSpinned = true;
            AnswerGiven = true;
            for (var i = 0; i < WordLetters.Count; i++)
            {
                WordLetters[i].Text = info.Word![i].ToString();
            }
            Info = client.IsTurn 
                ? "" 
                : $"Ход: {client.SessionInfo.CurrentPlayer.Name}";
            if (info.IsGuessed)
                _client.Player.Points += PotentialScore;
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
    }
    
    private async void TextBox_OnTextInput(object sender, RoutedEventArgs e)
    {
        var text = (sender as TextBox)!.Text;
        if (string.IsNullOrEmpty(text))
            return;
        if (LetterChosen && !_answerGiven)
        {
            var letter = text.ToCharArray()[0];
            AnswerGiven = false;
            // Task.Run(async() => await _client.ReportLetter(letter));
            await _client.ReportLetter(letter, PotentialScore);
            WordInput.Text = "";
        }

        if (WordChosen && !_answerGiven)
        {
            if (text.Length < WordLetters.Count)
                return;
            AnswerGiven = false;
            await _client.ReportWord(text, PotentialScore);
            WordInput.Text = "";
        }
    }

    private void OpenLetter(object sender, RoutedEventArgs e)
    {
        LetterChosen = true;
    }

    private void OpenWord(object sender, RoutedEventArgs e)
    {
        WordChosen = true;
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
            PotentialScore = scores[num];
            TargetAngle = 1080 + num * 45 - 22.5;
            RotateImage.Visibility = Visibility.Visible;
            await Task.Delay(5*1000);
            RotateImage.Visibility = Visibility.Collapsed;
            WheelSpinned = false;
        });
    }
}