using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Ambinity.Installer.Models;
using Ambinity.Installer.Services;
using Ambinity.Installer.Utilities;
using AmbinityServer.AppRelease;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.Styling;
using Newtonsoft.Json;
using Serilog;

namespace Ambinity.Installer.ViewModels;

public class RootViewModel : ViewModelBase
{
    private readonly InstallationService _installationService;
    private readonly UninstallViewModel _uninstallViewModel;
    private readonly InstallViewModel _installViewModel;
    private readonly ModifyViewModel _modifyViewModel;

    public event Action Close;
    public RootViewModel(InstallationService installationService, InstallViewModel installViewModel,UninstallViewModel uninstallViewModel,ModifyViewModel modifyViewmodel)
    {
        _modifyViewModel = modifyViewmodel;
        _installationService = installationService;
        _installViewModel = installViewModel;
        _uninstallViewModel = uninstallViewModel;
        _installViewModel.CloseWindowRequested += OnCloseWindowRequested;
        _uninstallViewModel.CloseWindowRequested += OnCloseWindowRequested;
       

    }

    private void OnCloseWindowRequested()
    {
        Close?.Invoke();
    }

    public void Init()
    {
        if (_installationService.Args!=null && _installationService.Args.First() == "-uninstall")
        {
            Log.Information("Uninstall mode");
            _uninstallViewModel.Init();
            CurrentInstallMode = _uninstallViewModel;
        }
       
        else
        {
            if (_installationService.GetInstallKey() != null)
            {
                CurrentInstallMode = _modifyViewModel;
                _modifyViewModel.ModifyOptionCommited += OnModifyOptionCommited;
                _modifyViewModel.CustomVersionSelected += OnCustomVersionSelected;
            }
            else
            {
                _installViewModel.Init();
                CurrentInstallMode = _installViewModel;
            }
          
        }

    }

    private void OnCustomVersionSelected(AppReleaseInformation info)
    {
        _installViewModel.Init();
        CurrentInstallMode = _installViewModel;   
        _installViewModel.InstallCustomVersion(info);
    }

    private void OnModifyOptionCommited(string option)
    {
        switch (option)
        {
            case "uninstall":
                _uninstallViewModel.Init();
                CurrentInstallMode = _uninstallViewModel;   
                break;
            case "update":
                _installViewModel.Init();
                CurrentInstallMode = _installViewModel;   
                break;
      
        }
    }

    private ViewModelBase _currentInstallMode;

    public ViewModelBase CurrentInstallMode
    {
        get => _currentInstallMode;
        set
        {
            _currentInstallMode = value;
            OnPropertyChanged();
        }
    }
}