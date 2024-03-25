using FileDBAvalonia;

namespace FileDBAvaloniaTests;

record Event();

[TestClass]
public class EventsTests
{
    private readonly EventRecorder recorder = new();

    [TestInitialize]
    public void Init()
    {
        Bootstrapper.Reset();

        recorder.Reset();
        recorder.Record<Event>();
    }

    [TestMethod]
    public void Send()
    {
        var event1 = new Event();
        Messenger.Send(event1);
        recorder.AssertEventsRecorded<Event>(1);

        var event2 = new Event();
        Messenger.Send(event2);
        recorder.AssertEventsRecorded<Event>(2);

        var recording = recorder.GetRecording<Event>().ToList();
        Assert.AreSame(event1, recording[0]);
        Assert.AreSame(event2, recording[1]);
    }

    [TestMethod]
    public void Send_TypeArg()
    {
        Messenger.Send<Event>();
        recorder.AssertEventsRecorded<Event>(1);

        Messenger.Send<Event>();
        recorder.AssertEventsRecorded<Event>(2);
    }
}
