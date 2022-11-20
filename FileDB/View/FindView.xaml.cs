using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Model;
using FileDB.ViewModel;
using LibVLCSharp.Shared;

namespace FileDB.View;

/// <summary>
/// Interaction logic for FindView.xaml
/// </summary>
public partial class FindView : UserControl
{
    private LibVLC libVLC;
    private LibVLCSharp.Shared.MediaPlayer mediaPlayer;

    public FindView()
    {
        InitializeComponent();
        DataContext = FindViewModel.Instance;

        WeakReferenceMessenger.Default.Register<ShowImage>(this, (r, m) =>
        {
            var transformBmp = new TransformedBitmap();
            transformBmp.BeginInit();
            transformBmp.Source = m.Image;
            transformBmp.Transform = new RotateTransform(m.RotateDegrees);
            transformBmp.EndInit();

            CurrentFileImage.Source = transformBmp;
        });

        WeakReferenceMessenger.Default.Register<ShowVideo>(this, (r, m) =>
        {
            CurrentFileVideo.MediaPlayer = mediaPlayer;
            mediaPlayer.Play(new Media(libVLC, new Uri(m.Path)));
        });

        WeakReferenceMessenger.Default.Register<CloseFile>(this, (r, m) =>
        {
            CurrentFileImage.Source = null;

            // TODO: what more is required to hide the media player? Set visibility via viewmodel binding?
            mediaPlayer?.Stop();
            CurrentFileVideo.MediaPlayer = null;
        });
    }

    private void VideoView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        libVLC = new();
        mediaPlayer = new(libVLC);
    }
}
