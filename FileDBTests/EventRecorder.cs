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

    public void AssertEventRecorded<T>(int? numEvents = null)
    {
        if (numEvents == null)
        {
            Assert.IsTrue(events.Any(x => x is T));
        }
        else
        {
            Assert.AreEqual(numEvents, events.Count(x => x is T));
        }
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

    public void AssertEventRecorded(int? numEvents = null)
    {
        AssertEventRecorded<T>(numEvents);
    }
}
