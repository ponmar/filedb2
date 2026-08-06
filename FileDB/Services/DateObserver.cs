using System;
using Avalonia.Threading;
using FileDB.Infrastructure;
using FileDB.Model;

namespace FileDB.Services;

public class DateObserver : IDisposable
{
    private DateTime date;
    private readonly IDateTimeProvider dateTimeProvider;

    private readonly DispatcherTimer dateCheckerTimer;

    public DateObserver(IDateTimeProvider dateTimeProvider)
    {
        this.dateTimeProvider = dateTimeProvider;
        date = dateTimeProvider.Now;
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

    public void CheckDate()
    {
        var now = dateTimeProvider.Now;
        if (date.Date != now.Date)
        {
            date = now;
            Messenger.Send<DateChanged>();
        }
    }

    private void DateCheckerTimer_Tick(object? sender, EventArgs e)
    {
        CheckDate();
    }
}
