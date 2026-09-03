using FileDB.Infrastructure;
using Xunit;

namespace FileDB.Tests;

record Event();

public class EventRecorderTests
{
    private readonly EventRecorder recorder;

    public EventRecorderTests()
    {
        recorder = new();
        recorder.Reset();
        recorder.Record<Event>();
    }

    [Fact]
    public void Send()
    {
        var event1 = new Event();
        Messenger.Send(event1);
        recorder.AssertEventsRecorded<Event>(1);

        var event2 = new Event();
        Messenger.Send(event2);
        recorder.AssertEventsRecorded<Event>(2);

        var recording = recorder.GetRecording<Event>().ToList();
        Assert.Same(event1, recording[0]);
        Assert.Same(event2, recording[1]);
    }

    [Fact]
    public void Send_TypeArg()
    {
        Messenger.Send<Event>();
        recorder.AssertEventsRecorded<Event>(1);

        Messenger.Send<Event>();
        recorder.AssertEventsRecorded<Event>(2);
    }
}
