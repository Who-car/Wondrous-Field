using System.ComponentModel;

namespace ClientFrontend.UIElementHelpers;

public sealed class CellContent : INotifyPropertyChanged
{
    private string _text;
    private bool _isEnabled;
    public string Text
    {
        get => _text;
        set
        {
            if (_text == value) return;
            _text = value;
            OnPropertyChanged(nameof(Text));
        }
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value) return;
            _isEnabled = value;
            OnPropertyChanged(nameof(IsEnabled));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}