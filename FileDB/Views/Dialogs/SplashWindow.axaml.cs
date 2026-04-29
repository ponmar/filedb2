using Avalonia.Controls;
using System.Threading;

namespace FileDB.Views.Dialogs;

public partial class SplashWindow : Window
{
    public SplashWindow()
    {
        InitializeComponent();
    }

    public void EnableCancellation(CancellationTokenSource cts)
    {
        CancelButton.IsVisible = true;
        CancelButton.Click += (_, _) => cts.Cancel();
    }
}
