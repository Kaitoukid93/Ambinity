using System.Windows.Input;
using Ambinity.Stores;
using Ambinity.ViewModels;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.Screens.Dashboard;
using Ambinity.Views.Screens.ProfileEditor;
using Ambinity.Views.SideMenu;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;


namespace Ambinity.Views
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        #region Construct

        public MainWindowViewModel(RootNavigationStores rootNavigationStores,SideMenuViewModel sideMenu)
        {
            _rootNavigationStores = rootNavigationStores;
            CommandSetup();
            SideMenu = sideMenu;
            sideMenu.SelectedProfileChanged += OnSelectedProfileChanged;
            sideMenu.SelectedScreenChanged += OnSelectedScreenChanged;
            sideMenu.Init();
            //load navigation view

        }

        private void OnSelectedScreenChanged(SideMenuScreenViewModel screen)
        {
            switch (screen.Content)
            {
                case "Dashboard":
                    GoToDashBoard();
                    break;
                case "Devices":
                   // GoToSurfaceEditor();
                    break;
                case "Settings":
                    //gotosettings
                    break;
            }
        }

        private void OnSelectedProfileChanged(SideMenuProfileViewModel profile)
        {
           // throw new System.NotImplementedException();
           GoToProfileEditor(profile);
        }

        #endregion

        #region Events

        #endregion

        #region Properties

        private SideMenuViewModel _sideMenu;

        public SideMenuViewModel SideMenu
        {
            get
            {
                return _sideMenu;
            }
            set
            {
                _sideMenu = value;
                RaisePropertyChanged(nameof(SideMenu));
            }
        }
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
            //var vm = Ioc.Default.GetRequiredService<Draw2DCanvasViewModel>();
          //  vm.Init();
           // _rootNavigationStores.CurrentViewModel = vm;
        }

        private void GoToDashBoard()
        {
            var vm = Ioc.Default.GetRequiredService<DashboardViewModel>();
            vm.Init();
            _rootNavigationStores.CurrentViewModel = vm;
        }
        private void GoToProfileEditor(SideMenuProfileViewModel profile)
        {
            var vm = Ioc.Default.GetRequiredService<ProfileEditorViewModel>();
              vm.Init(profile.Profile);
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