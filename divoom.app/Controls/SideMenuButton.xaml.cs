using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace divoom.app;

public sealed partial class SideMenuButton : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(SideMenuButton),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty GlyphProperty =
        DependencyProperty.Register(nameof(Glyph), typeof(string), typeof(SideMenuButton),
            new PropertyMetadata(string.Empty));


    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(SideMenuButton),
            new PropertyMetadata(false, OnIsSelectedChanged));

    private bool _pointerOver;

    public SideMenuButton()
    {
        InitializeComponent();
        PointerEntered  += (_, _) => { _pointerOver = true;  VisualStateManager.GoToState(this, "PointerOver", true); };
        PointerExited   += (_, _) => { _pointerOver = false; VisualStateManager.GoToState(this, "Normal", true); };
        PointerPressed  += (_, _) => VisualStateManager.GoToState(this, "Pressed", true);
        PointerReleased += (_, _) => VisualStateManager.GoToState(this, _pointerOver ? "PointerOver" : "Normal", true);
        Tapped += OnTapped;
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var btn = (SideMenuButton)d;
        VisualStateManager.GoToState(btn, (bool)e.NewValue ? "Selected" : "Unselected", true);
    }

    private void OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var parent = Parent as FrameworkElement;
        while (parent is not null and not SideMenu)
            parent = parent.Parent as FrameworkElement;

        (parent as SideMenu)?.OnSideMenuButtonClicked(this);
    }
}
