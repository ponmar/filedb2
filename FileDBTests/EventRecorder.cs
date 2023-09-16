using FileDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace FileDBTests;

public class EventRecorder
{
    private readonly List<object> events = new();

    public void Record<T>() where T : class
    {
        this.RegisterForEvent<T>(events.Add);
    }

    public void Reset()
    {
        events.Clear();
    }

    public IEnumerable<T> GetRecording<T>() where T : class
    {
        return events.Where(x => x is T).Cast<T>();
    }

    public T AssertEventRecorded<T>()
    {
        return AssertEventsRecorded<T>(1).Single();
    }

    public IEnumerable<T> AssertEventsRecorded<T>(int numEvents)
    {
        var matchingEvents = events.OfType<T>();
        Assert.AreEqual(numEvents, events.Count);
        return matchingEvents;
    }

    public void AssertNoEventsRecorded()
    {
        Assert.IsTrue(!events.Any());
    }
}

public class SingleEventRecorder<T> : EventRecorder where T : class
{
    public SingleEventRecorder()
    {
        Record<T>();
    }

    public T AssertEventRecorded()
    {
        return AssertEventsRecorded<T>(1).Single();
    }

    public IEnumerable<T> AssertEventsRecorded(int numEvents)
    {
        return AssertEventsRecorded<T>(numEvents);
    }
}
