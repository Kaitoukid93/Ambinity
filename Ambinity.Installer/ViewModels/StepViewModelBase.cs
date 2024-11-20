namespace Ambinity.Installer.ViewModels;

public abstract class StepViewModelBase : ViewModelBase
{
    public virtual void Init()
    {
        //setup dependency service or something??
    }
    public int StepIndex { get; set; }
    public bool CanBack { get; set; }
    public bool CanCancel { get; set; }
    public bool CanForward { get; set; }
    public bool IsBusy { get; set; }
    
}