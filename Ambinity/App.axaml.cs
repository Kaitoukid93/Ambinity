using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using HotAvalonia;

namespace Ambinity
{
    public partial class App : Application
    {
        private bool _shutDown;
        public override void Initialize()
        {
            //check if any instance of ambinity is running
            if (FocusExistingInstance())
            {
                _shutDown = true;
                Environment.Exit(1);
            }
            //hot reload with jetbrains rider
            this.EnableHotReload(); // Ensure this line **precedes** `AvaloniaXamlLoader.Load(this);`
            AvaloniaXamlLoader.Load(this);
        }
        
        private bool FocusExistingInstance()
        {
            if (Design.IsDesignMode)
                return false;
            _ambinityMutex = new Mutex(true, "Ambinity-3c24b502-64e6-4587-84bf-9072970e535f", out bool createdNew);
            return !createdNew;
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                return;
            BindingPlugins.DataValidators.RemoveAt(0);
            //register service and ui
            AmbinityBootStrapper.Initialize(this);
            
           
        }
        
        private Mutex? _ambinityMutex;
    }
}