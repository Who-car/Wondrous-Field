using System.Windows;
using System.Windows.Controls;

namespace ClientFrontend.UIElementHelpers;

public class CellSelectorTemplate : DataTemplateSelector
{
    public DataTemplate FilledCellTemplate { get; set; }
    public DataTemplate EmptyCellTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is CellContent wordLetter && !string.IsNullOrWhiteSpace(wordLetter.Text))
            return FilledCellTemplate;
        return EmptyCellTemplate;
    }
}