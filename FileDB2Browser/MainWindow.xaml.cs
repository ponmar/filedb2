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
using FileDB2Browser.Config;
using FileDB2Browser.ViewModel;
using FileDB2Interface;

namespace FileDB2Browser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            if (!FileDB2BrowserConfigIO.FileExists())
            {
                if (!FileDB2BrowserConfigIO.Write(new FileDB2BrowserConfig()))
                {
                    Utils.ShowErrorDialog("Unable to write configuration file: " + FileDB2BrowserConfigIO.GetFilePath());
                }
            }
        }
    }
}
