using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.CapturingService;
using AmbinityCore.LightingEngines;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class MotionConfigParameterViewModel : ParameterViewModelBase
{
    private Dictionary<string, MotionTypeEnum> _typeEnums = new Dictionary<string, MotionTypeEnum>();

    public MotionConfigParameterViewModel(SelfGeneratedColorConfiguration configuration,
        CapturingServiceProvider capturingServiceProvider,
        ProfileEditorRightPanelViewModel rightPanelViewModel, BrightnessProviderFactory brightnessProviderFactory
    )
    {
        _typeEnums.Add("Music Reactive", MotionTypeEnum.MusicReactive);
        _typeEnums.Add("None", MotionTypeEnum.None);
        _typeEnums.Add("Breathing", MotionTypeEnum.Breathing);
        _capturingServiceProvider = capturingServiceProvider;
        _configuration = configuration;
        _rightPanelViewModel = rightPanelViewModel;
        _brightnessProviderFactory = brightnessProviderFactory;
        AvailableMotions = new List<string>()
        {
            "None",
            "Music Reactive",
            "Breathing"
        };
        _selectedMotionConfig = AvailableMotions.First();
        _selectedMotionConfig = _typeEnums.Where(x => x.Value == _configuration.MotionConfig.Type).FirstOrDefault().Key;
        OpenConfigCommand = new RelayCommand(OpenConfig);
    }

    private BrightnessProviderFactory _brightnessProviderFactory;

    private void OpenConfig()
    {
        //todo using viewmodel factory to create configViewmodel
        var configViewModel = MotionConfigurationViewModelFactory(_configuration.MotionConfig);
        _rightPanelViewModel.OpenFlyout(configViewModel);
    }

    private MotionConfigurationViewModelBase MotionConfigurationViewModelFactory(IMotionConfiguration configuration)
    {
        switch (configuration.Type)
        {
            case MotionTypeEnum.Breathing:
                return new BreathingColorConfigurationViewModel(configuration);
            case MotionTypeEnum.MusicReactive:
                return new MusicReactiveConfigurationViewModel(configuration, _capturingServiceProvider);
            default: return null;
        }
    }

    private ProfileEditorRightPanelViewModel _rightPanelViewModel;
    private SelfGeneratedColorConfiguration _configuration;
    public List<string> AvailableMotions { get; set; }
    private string _selectedMotionConfig;
    private readonly CapturingServiceProvider _capturingServiceProvider;

    public string SelectedMotionConfig
    {
        get => _selectedMotionConfig;
        set
        {
            _selectedMotionConfig = value;
            MotionTypeEnum type = MotionTypeEnum.None;
            _typeEnums.TryGetValue(value, out type);
            _configuration.MotionConfig = GetMotionConfig(type);
            _configuration.UpdateMotionConfig();
            OnPropertyChanged(nameof(ShowSettingsButton));
            OnPropertyChanged();
        }
    }

    public bool ShowSettingsButton => _configuration.MotionConfig.Type == MotionTypeEnum.Breathing ||
                                      _configuration.MotionConfig.Type == MotionTypeEnum.MusicReactive;

    public ICommand OpenConfigCommand { get; set; }

    public class NullMotionConfigViewModel : MotionConfigurationViewModelBase
    {
        public NullMotionConfigViewModel(IMotionConfiguration config)
        {
            Configuration = config;
            Type = config.Type;
            Name = "None";
            Description = "Apply no effect on the colors";
            ShowSettingsButton = false;
        }

        public string Icon { get; set; }
    }

    public IMotionConfiguration GetMotionConfig(MotionTypeEnum type)
    {
        switch (type)
        {
            case MotionTypeEnum.MusicReactive:
                return new MusicReactiveMotionConfiguration();
            case MotionTypeEnum.Breathing:
                return new BreathingMotionConfiguration();
            case MotionTypeEnum.None:
                return new NoneMotionConfiguration();
            default:
                return null;
        }
    }

    public override void Dispose()
    {
    }
}