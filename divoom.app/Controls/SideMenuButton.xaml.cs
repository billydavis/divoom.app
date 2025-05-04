using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace divoom.app;

public sealed partial class SideMenuButton : UserControl, INotifyPropertyChanged
{
    private string _text;
    private string _glyph;

    public SideMenuButton()
    {
        this.InitializeComponent();
        this.Tapped += OnSideMenuButtonTapped;
    }

    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }
    }

    public string Glyph
    {
        get => _glyph;
        set
        {
            if (_glyph != value)
            {
                _glyph = value;
                OnPropertyChanged(nameof(Glyph));
            }
        }
    }

    private void OnSideMenuButtonTapped(object sender, TappedRoutedEventArgs e)
    {
        // Raise a custom event or call a method in the parent SideMenu
        var parent = this.Parent as FrameworkElement;
        while (parent != null && !(parent is SideMenu))
        {
            parent = parent.Parent as FrameworkElement;
        }

        if (parent is SideMenu sideMenu)
        {
            sideMenu.OnSideMenuButtonClicked(this);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}