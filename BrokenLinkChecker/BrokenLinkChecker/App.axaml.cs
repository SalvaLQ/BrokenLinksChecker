using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using BrokenLinkChecker.ViewModels;
using BrokenLinkChecker.Views;
using System.Text;
using System;

namespace BrokenLinkChecker;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
       
    }

    public override void OnFrameworkInitializationCompleted()
    {

        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);
        MainWnd MainWnd = new MainWnd();
        MainWnd.DataContext = new MainVM(MainWnd);
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {            
            desktop.MainWindow = MainWnd;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = MainWnd;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
