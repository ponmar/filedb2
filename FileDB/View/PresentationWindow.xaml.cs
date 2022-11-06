﻿using CommunityToolkit.Mvvm.Messaging;
using FileDB.Model;
using FileDB.ViewModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FileDB.View;

/// <summary>
/// Interaction logic for PresentationWindow.xaml
/// </summary>
public partial class PresentationWindow : Window
{
    public PresentationWindow()
    {
        InitializeComponent();
        DataContext = FindViewModel.Instance;

        WeakReferenceMessenger.Default.Register<ShowImage>(this, (r, m) => ShowImage(m.Image, m.RotateDegrees));

        WeakReferenceMessenger.Default.Register<CloseImage>(this, (r, m) =>
        {
            CurrentFileImage.Source = null;
        });
    }

    public void ShowImage(BitmapImage image, double rotateDegrees)
    {
        var transformBmp = new TransformedBitmap();
        transformBmp.BeginInit();
        transformBmp.Source = image;
        transformBmp.Transform = new RotateTransform(rotateDegrees);
        transformBmp.EndInit();

        CurrentFileImage.Source = transformBmp;
    }
}
