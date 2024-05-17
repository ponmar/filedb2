using System;
using FileDBInterface.Utils;
using Xunit;

namespace FileDBInterfaceTests;

public class TimeUtilsTests
{
    [Fact]
    public void GetAgeInYears()
    {
        Assert.Equal(0, TimeUtils.GetAgeInYears(new DateTime(2021, 5, 23), new DateTime(2020, 5, 24)));
        Assert.Equal(1, TimeUtils.GetAgeInYears(new DateTime(2021, 5, 23), new DateTime(2020, 5, 23)));
        Assert.Equal(0, TimeUtils.GetAgeInYears(new DateTime(1902, 4, 4), new DateTime(1902, 1, 1)));
        Assert.Equal(0, TimeUtils.GetAgeInYears(new DateTime(1902, 4, 4), new DateTime(1902, 7, 6)));
        Assert.Equal(0, TimeUtils.GetAgeInYears(new DateTime(2022, 10, 15, 11, 44, 0), new DateTime(2022, 10, 15, 11, 44, 0)));
    }

    [Fact]
    public void GetYearsAgo()
    {
        Assert.Equal(0, TimeUtils.GetYearsAgo(new DateTime(2021, 5, 23), new DateTime(2020, 5, 24)));
        Assert.Equal(1, TimeUtils.GetYearsAgo(new DateTime(2021, 5, 23), new DateTime(2020, 5, 23)));
    }

    [Fact]
    public void GetDaysToNextBirthday()
    {
        var tomorrow = DateTime.Today + TimeSpan.FromDays(1);
        Assert.Equal(1, TimeUtils.GetDaysToNextBirthday(tomorrow));
    }
}
