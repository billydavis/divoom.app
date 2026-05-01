using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace divoom.app;

public class HardwareIconSelector : DataTemplateSelector
{
    public DataTemplate Pixoo64Template { get; set; }
    public DataTemplate TimesGateTemplate { get; set; }
    public DataTemplate DefaultTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) =>
        item is int hw ? hw switch
        {
            92  => Pixoo64Template,
            400 => TimesGateTemplate,
            _   => DefaultTemplate
        } : DefaultTemplate;
}
