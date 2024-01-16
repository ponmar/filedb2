using FileDBShared;

namespace FileDBSharedTests;

[TestClass]
public class TimeUtilsTests
{
    [TestMethod]
    public void GetAgeInYears()
    {
        Assert.AreEqual(0, TimeUtils.GetAgeInYears(new DateTime(2021, 5, 23), new DateTime(2020, 5, 24)));
        Assert.AreEqual(1, TimeUtils.GetAgeInYears(new DateTime(2021, 5, 23), new DateTime(2020, 5, 23)));
        Assert.AreEqual(0, TimeUtils.GetAgeInYears(new DateTime(1902, 4, 4), new DateTime(1902, 1, 1)));
        Assert.AreEqual(0, TimeUtils.GetAgeInYears(new DateTime(1902, 4, 4), new DateTime(1902, 7, 6)));
        Assert.AreEqual(0, TimeUtils.GetAgeInYears(new DateTime(2022, 10, 15, 11, 44, 0), new DateTime(2022, 10, 15, 11, 44, 0)));
    }

    [TestMethod]
    public void GetYearsAgo()
    {
        Assert.AreEqual(0, TimeUtils.GetYearsAgo(new DateTime(2021, 5, 23), new DateTime(2020, 5, 24)));
        Assert.AreEqual(1, TimeUtils.GetYearsAgo(new DateTime(2021, 5, 23), new DateTime(2020, 5, 23)));
    }

    [TestMethod]
    public void GetDaysToNextBirthday()
    {
        var tomorrow = DateTime.Today + TimeSpan.FromDays(1);
        Assert.AreEqual(1, TimeUtils.GetDaysToNextBirthday(tomorrow));
    }
}
