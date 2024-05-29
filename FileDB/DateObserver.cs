using System;
using Avalonia.Threading;
using FileDB.Model;

namespace FileDB;

public class DateObserver : IDisposable
{
    private DateTime date = DateTime.Now;

    private readonly DispatcherTimer dateCheckerTimer;

    public DateObserver()
    {
        dateCheckerTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        dateCheckerTimer.Tick += DateCheckerTimer_Tick;
        dateCheckerTimer.Start();
    }

    public void Dispose()
    {
        dateCheckerTimer.Stop();
    }

    private void DateCheckerTimer_Tick(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        if (date.Date != now.Date)
        {
            date = now;
            Messenger.Send<DateChanged>();
        }
    }
}
