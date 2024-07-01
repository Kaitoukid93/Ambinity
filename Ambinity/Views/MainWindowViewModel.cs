using System.Windows.Input;
using Ambinity.Stores;
using Ambinity.ViewModels;
using Ambinity.Views.Screens.Dashboard;
using Ambinity.Views.Screens.DeviceControl.DeviceCanvas;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;


namespace Ambinity.Views
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        #region Construct

        public MainWindowViewModel(RootNavigationStores rootNavigationStores)
        {
            _rootNavigationStores = rootNavigationStores;
            CommandSetup();
        }

        #endregion

        #region Events

        #endregion

        #region Properties

        private readonly RootNavigationStores _rootNavigationStores;
        public ViewModelBase CurrentViewModel => _rootNavigationStores.CurrentViewModel;

        #endregion

        #region Methods

        private void CommandSetup()
        {
            GoToSurfaceEditorCommand = new RelayCommand(GoToSurfaceEditor);
        }

        private void GoToSurfaceEditor()
        {
            var vm = Ioc.Default.GetRequiredService<DeviceCanvasViewModel>();
            vm.Init();
            _rootNavigationStores.CurrentViewModel = vm;
        }

        #endregion

        #region Command

        public ICommand GoToSurfaceEditorCommand { get; set; }

        #endregion

        #region Events

        #endregion


        // public INavigationService NavigationService;
    }
}