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
using System.Windows.Navigation;
using System.Windows.Shapes;
using HandyControl.Data;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for UserIncreamentInput.xaml
    /// </summary>
    public partial class UserIncreamentInput : UserControl
    {
        public UserIncreamentInput()
        {
            InitializeComponent();
            StartPoint.VerifyFunc = str => double.TryParse(str, out var v)
                ? v > StartPoint.Maximum
                    ? OperationResult.Failed("Error")
                    : OperationResult.Success()
                : OperationResult.Failed("Error");
            EndPoint.VerifyFunc = str => double.TryParse(str, out var v)
               ? v > StartPoint.Maximum
                   ? OperationResult.Failed("Error")
                   : OperationResult.Success()
               : OperationResult.Failed("Error");
        }
    }
}