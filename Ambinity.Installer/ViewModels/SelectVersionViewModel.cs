using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ambinity.Installer.Services;
using AmbinityServer;
using AmbinityServer.AppRelease;
using Serilog;
using Ambinity.Installer.Localization;
namespace Ambinity.Installer.ViewModels;

public class SelectVersionViewModel : StepViewModelBase
{
    private readonly InstallationService _installationService;

    public SelectVersionViewModel(InstallationService service)
    {
        CanForward = true;
        CanCancel = false;
        CanBack = true;
        StepIndex = 1;
        _installationService = service;
        AvailableRelease = new ObservableCollection<AppReleaseInfomationViewModel>();
        Loc.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(SubHeader));
    }

    public string Header => Loc.Get("SelectVersion.Header.Content");
    public string SubHeader => Loc.Get("SelectVersion.SubHeader.Content");
    public ObservableCollection<AppReleaseInfomationViewModel> AvailableRelease { get; set; }

    public AppReleaseInformation SelectedVersion =>
        AvailableRelease.Where(r => r.IsSelected).FirstOrDefault().Information;

    public async Task Init()
    {
        AvailableRelease?.Clear();
        IsLoading = true;
        var availableRelease = await _installationService.GetAvailableRelease();
        if (availableRelease == null || availableRelease.Count==0)
        {
            Log.Warning("No release found");
            return;
        }
        foreach (var release in availableRelease.OrderByDescending(i=>i.ReleaseDate))
        {
            AvailableRelease.Add(new AppReleaseInfomationViewModel(release));
        }

        AvailableRelease.First().IsSelected = true;
        IsLoading = false;
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }
}
