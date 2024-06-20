using Ambinity.Stores;
using Ambinity.ViewModels;


namespace Ambinity.Views
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        #region Construct

        public MainWindowViewModel(NavigationStores navigationStores)
        {
            _navigationStores = navigationStores;
            _navigationStores.CurrentViewModelChanged += OnCurrentViewModelChanged;
            CommandSetup();
        }
  
        #endregion
        #region Events
        
        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }

        #endregion

        #region Properties

        private readonly NavigationStores _navigationStores;
        public ViewModelBase CurrentViewModel => _navigationStores.CurrentViewModel;


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