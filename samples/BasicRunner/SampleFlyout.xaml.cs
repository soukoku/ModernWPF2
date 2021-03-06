﻿using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace BasicRunner
{
    /// <summary>
    /// Interaction logic for SampleFlyout.xaml
    /// </summary>
    public partial class SampleFlyout : Flyout
    {
        public SampleFlyout()
        {
            InitializeComponent();
        }

        protected override void OnClosing()
        {
            Debug.WriteLine("Sample flyout closing.");
        }
        protected override void OnClosed()
        {
            Debug.WriteLine("Sample flyout closed.");
        }
    }
}
