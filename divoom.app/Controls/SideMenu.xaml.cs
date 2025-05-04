using System;
using divoom.app.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace divoom.app;

public sealed partial class SideMenu : UserControl
{
    public event EventHandler<SideMenuButton> SideMenuButtonClicked;

    public event EventHandler<NavigationChangeEvent> NavigationChange;

    public SideMenu()
    {
        this.InitializeComponent();
    }
        

    public void OnSideMenuButtonClicked(SideMenuButton button)
    {
        // SideMenuButtonClicked?.Invoke(this, button);
        NavigationChange?.Invoke(this, new NavigationChangeEvent { Page = button.Text });
    }

}