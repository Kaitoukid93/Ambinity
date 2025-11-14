using System.Threading.Tasks;
using Ambinity.ViewModels;

namespace Ambinity.Views.Screens
{
    public class ScreenViewModelBase : ViewModelBase
    {

        /// <summary>
        /// Initialize the screen before showing
        /// </summary>
        /// <returns></returns>
        public virtual async Task Init()
        {

        }
    }

}
