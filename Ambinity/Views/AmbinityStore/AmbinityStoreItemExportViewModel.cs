using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.ViewModels;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityServer.OnlineItem;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace Ambinity.Views.AmbinityStore;

public class AmbinityStoreItemExportViewModel : ViewModelBase
{
    private OnlineItem _onlineItem;
    private ICollectableItem _item;
    private IWindowService _windowService;
    public string Type => _onlineItem.Type.ToString();

    public AmbinityStoreItemExportViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        MDText = "[%{color:red}Đây là văn bản mẫu, chỉnh sửa lại cho phù hợp với tài nguyên%]\n\n" +
                 "##### **Cách sử dụng [Tên tài nguyên]**\n\n" +
                 " * **Tải xuống** \n" +
                 " Tải xuống tài nguyên này bằng cách nhấn vào nút download\n" +
                 " * **Sử dụng** \n" +
                 " Tạo mới hoặc nhấp vào một Color Zone bất kỳ\n" +
                 " Chọn Fill Color - nhấn vào icon Thư viện sau đó chọn mục vừa tải\n \n" +
                 "Learn more about color palette at [Ambino-ColorPalette](https://ambino.vn)";
        ExportItemForServerCommand = new AsyncRelayCommand(ExportItemForServer);
        ExportItemLocallyCommand = new AsyncRelayCommand(ExportLocally);
        OpenScreenShotsPickerCommand = new AsyncRelayCommand(OpenScreenshotsPicker);
        OpenThumbnailPickerCommand = new AsyncRelayCommand(OpenThumbnailPicker);
        ScreenShots = [];
    }

    private async Task OpenThumbnailPicker()
    {
        string[]? result = await _windowService.CreateOpenFileDialog()
            .HavingFilter(f => f.WithExtension("png").WithName("png image"))
            .ShowAsync();
        if (result == null)
            return;
        Thumbnail = result[0];
    }

    private async Task OpenScreenshotsPicker()
    {
        string[]? result = await _windowService.CreateOpenFileDialog()
            .HavingFilter(f => f.WithExtension("png").WithName("png image"))
            .HavingFilter(f => f.WithExtension("jpg").WithName("jpg image"))
            .ShowAsync();
        if (result == null)
            return;
        foreach (var file in result)
        {
            ScreenShots.Add(file);
        }
    }

    private void SaveItemData()
    {
        _item.Name = Name;
        _onlineItem.Name = Name;
        _onlineItem.Owner = Author;
        _onlineItem.Version = Version;
        _onlineItem.Description = Description;
        _item.Save();
    }

    private async Task ExportLocally()
    {
        SaveItemData();
        string? result = await _windowService.CreateSaveFileDialog()
            .HavingFilter(f => f.WithExtension("zip"))
            .WithInitialFileName(Name)
            .ShowAsync();
        if (result == null)
            return;
        if(File.Exists(result))
            File.Delete(result);
        ZipFile.CreateFromDirectory(_item.LocalPath, result);
        Log.Information("Item exported to " + result);
    }

    public void Init(ICollectableItem item)
    {
        _item = item;
        Name = _item.Name;
        _onlineItem = new OnlineItem
        {
            Name = _item.Name,
            Type = _item.GetType()
        };
    }
    private async Task ExportItemForServer()
    {
        SaveItemData();
        string? result = await _windowService.CreateSaveFileDialog()
            .WithInitialFileName(_item.Name)
            .ShowAsync();
        if (result == null)
            return;
        //create file structure
        //...
        ////content
        ////screenshots
        ////description.md
        ////info.json
        ////thumb.png
        var workingDir = Path.Combine(Directory.GetParent(result).ToString(), _item.Name);
        var contentDir = Path.Combine(workingDir, "content");
        var screenshotsDir = Path.Combine(workingDir, "screenshots");
        var descriptionDir = Path.Combine(workingDir, "description.md");
        var infoDir = Path.Combine(workingDir, "info.json");
        var thumbnailDir = Path.Combine(workingDir, "thumbnails.png");
        Directory.CreateDirectory(workingDir);
        Directory.CreateDirectory(contentDir);
        Directory.CreateDirectory(screenshotsDir);
        //write content
        LocalFileHelpers.CopyDirectory(_item.LocalPath, contentDir,true);
        //copy screenshots
        foreach (var screenShot in ScreenShots)
        {
            try
            {
                File.Copy(screenShot,Path.Combine(screenshotsDir,Path.GetFileName(screenShot)));
            }
            catch (Exception e)
            {
                Log.Warning(e.Message);
                throw;
            }
            
        }
        //write description
        await File.WriteAllTextAsync(descriptionDir, MDText);
        //write info
        JsonHelpers.WriteSimpleJson(_onlineItem, infoDir);
        //copy thumbnail
        if (File.Exists(Thumbnail))
        {
            File.Copy(Thumbnail, thumbnailDir);
        }
        Log.Information("Item exported to " + result);
    }

    private string _thumbnail;

    public string Thumbnail
    {
        get => _thumbnail;
        set
        {
            _thumbnail = value;
            OnPropertyChanged();
        }
    }

    private List<string> _screenShots;

    public List<string> ScreenShots
    {
        get => _screenShots;
        set
        {
            _screenShots = value;
            OnPropertyChanged();
        }
    }

    private string _mdText =
        "This is a multi-line display\n    that has returns in it.\n    The text block respects the line breaks\n    as set out in XAML.";

    public string MDText
    {
        get => _mdText;
        set
        {
            _mdText = value;
            OnPropertyChanged();
        }
    }

    private string _version = "1.0.0";

    public string Version
    {
        get => _version;
        set
        {
            _version = value;
            OnPropertyChanged();
        }
    }

    private string _author = "Ambino";

    public string Author
    {
        get => _author;
        set
        {
            _author = value;
            OnPropertyChanged();
        }
    }

    private string _description = " Fill some short description";

    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            OnPropertyChanged();
        }
    }

    private string _name;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }
    public ICommand ExportItemForServerCommand { get; set; }
    public ICommand OpenThumbnailPickerCommand { get; }
    public ICommand OpenScreenShotsPickerCommand { get; }
    public ICommand ExportItemLocallyCommand { get; }
}