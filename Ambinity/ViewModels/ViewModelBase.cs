using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ambinity.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected string GetAssemblyResource(string name)
        {
            using (var stream = AssetLoader.Open(new Uri(name)))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
        
        public virtual void Dispose()
        {
            
        }
    }
    
}