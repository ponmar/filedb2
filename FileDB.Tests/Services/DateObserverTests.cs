using FakeItEasy;
using FileDB.Model;
using FileDB.Services;
using Xunit;

namespace FileDB.Tests.Services;

public class DateObserverTests
{
    [Fact]
    public void DateCheckerTimer_Tick_WhenDateChanges_SendsDateChanged()
    {
        var dateTimeProvider = A.Fake<IDateTimeProvider>();
        A.CallTo(() => dateTimeProvider.Now).ReturnsNextFromSequence(
            new DateTime(2026, 8, 4, 10, 0, 0),
            new DateTime(2026, 8, 5, 10, 0, 0));
        var recorder = new EventRecorder();
        recorder.Record<DateChanged>();

        var observer = new DateObserver(dateTimeProvider);
        try
        {
            observer.CheckDate();

            recorder.AssertEventRecorded<DateChanged>();
        }
        finally
        {
            observer.Dispose();
        }
    }

    [Fact]
    public void DateCheckerTimer_Tick_WhenDateIsUnchanged_DoesNotSendDateChanged()
    {
        var dateTimeProvider = A.Fake<IDateTimeProvider>();
        A.CallTo(() => dateTimeProvider.Now).Returns(new DateTime(2026, 8, 5, 10, 0, 0));
        var recorder = new EventRecorder();
        recorder.Record<DateChanged>();

        var observer = new DateObserver(dateTimeProvider);
        try
        {
            observer.CheckDate();

            recorder.AssertNoEventsRecorded();
        }
        finally
        {
            observer.Dispose();
        }
    }
}
