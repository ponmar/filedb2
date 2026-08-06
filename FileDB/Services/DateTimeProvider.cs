using System;

namespace FileDB.Services;

public interface IDateTimeProvider
{
    DateTime Now { get; }
}

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
}
