using Avalonia.Controls;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

namespace FileDB.Views.Dialogs;

[ExcludeFromCodeCoverage]
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
