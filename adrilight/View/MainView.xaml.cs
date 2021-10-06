﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using adrilight.ViewModel;
namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainViewViewModel ViewModel;
        public MainView()
        {
            InitializeComponent();
            // ViewModel = new MainViewViewModel();
            // this.DataContext = ViewModel;
            

        }

        
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {           
            this.DragMove();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var vm = DataContext as MainViewViewModel;
            if (vm != null)
            {
                vm.IsSettingsWindowOpen = false;
            }
        }


    }
}
