namespace Ambinity.Installer.ViewModels;

using Ambinity.Installer.Localization;
using Ambinity.Installer.Models;

public class WelcomeViewModel : StepViewModelBase
{
    public WelcomeViewModel()
    {
        StepIndex = 0;
        CanCancel = true;
        CanForward = true;
        CanBack = false;
        Loc.LanguageChanged += OnLanguageChanged;
        LanguageIndex = Loc.CurrentLanguage switch
        {
            "en" => 0,
            "vi" => 1,
            _ => 0
        };

        //get config files if exist and check language
    }

    private void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(SubHeader));
        OnPropertyChanged(nameof(AvailableLanguagesLabel));
    }
    public string Header => Loc.Get("Welcome.Header.Content");
    public string SubHeader => Loc.Get("Welcome.SubHeader.Content");
    public string AvailableLanguagesLabel => Loc.Get("Welcome.Language.Content");
    private int _languageIndex;

    public int LanguageIndex
    {
        get => _languageIndex;
        set
        {
            if (_languageIndex != value)
            {
                _languageIndex = value;
                string langCode = value switch
                {
                    0 => "en",
                    1 => "vi",
                    _ => "en"
                };
                Loc.Load(langCode);
                OnPropertyChanged();
            }
        }
    }
}
