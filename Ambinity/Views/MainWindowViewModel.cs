using Ambinity.Stores;
using Ambinity.ViewModels;


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
        }

        #endregion

        #region Command

        #endregion

        #region Events

        #endregion


        // public INavigationService NavigationService;
    }
}