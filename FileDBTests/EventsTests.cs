using FileDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace FileDBTests;

record Event();

[TestClass]
public class EventsTests
{
    private readonly EventRecorder recorder = new();

    [TestInitialize]
    public void Init()
    {
        recorder.Reset();
        recorder.Record<Event>();
    }

    [TestMethod]
    public void Send()
    {
        var event1 = new Event();
        Events.Send(event1);
        recorder.AssertEventRecorded<Event>(1);

        var event2 = new Event();
        Events.Send(event2);
        recorder.AssertEventRecorded<Event>(2);

        var recording = recorder.GetRecording<Event>().ToList();
        Assert.AreSame(event1, recording[0]);
        Assert.AreSame(event2, recording[1]);
    }

    [TestMethod]
    public void Send_TypeArg()
    {
        Events.Send<Event>();
        recorder.AssertEventRecorded<Event>(1);

        Events.Send<Event>();
        recorder.AssertEventRecorded<Event>(2);
    }
}
