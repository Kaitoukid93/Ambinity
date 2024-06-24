using CommunityToolkit.Mvvm.ComponentModel;

namespace Ambinity.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        public virtual void Init(object parameter)
        {
        }

        public virtual void Dispose()
        {
            
        }
    }
}