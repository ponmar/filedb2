using FakeItEasy;
using FileDB.Comparers;
using FileDB.ViewModels;
using FileDBInterface.Model;
using Xunit;

namespace FileDB.Tests.Comparers;

public class PersonsByDeceasedSorterTests
{
    private readonly ICriteriaViewModel fakeCriteria = A.Fake<ICriteriaViewModel>();

    private DeceasedPersonViewModel MakePerson(string deceased) =>
        new(fakeCriteria,
            new PersonModel
            {
                Id = 1,
                ShortName = "X",
                FullName = "X Y",
                DateOfBirth = "1950-01-01",
                Deceased = deceased,
            },
            null);

    [Fact]
    public void Compare_SameDeceasedDate_ReturnsZero()
    {
        var sut = new PersonsByDeceasedSorter();
        var a = MakePerson("2000-06-15");
        var b = MakePerson("2000-06-15");

        Assert.Equal(0, sut.Compare(a, b));
    }

    [Fact]
    public void Compare_EarlierDeceasedDate_ReturnsNegative()
    {
        var sut = new PersonsByDeceasedSorter();
        var earlier = MakePerson("1990-01-01");
        var later = MakePerson("2000-01-01");

        Assert.True(sut.Compare(earlier, later) < 0);
    }

    [Fact]
    public void Compare_LaterDeceasedDate_ReturnsPositive()
    {
        var sut = new PersonsByDeceasedSorter();
        var earlier = MakePerson("1990-01-01");
        var later = MakePerson("2010-05-20");

        Assert.True(sut.Compare(later, earlier) > 0);
    }

    [Fact]
    public void Compare_IsUsedCorrectlyForSorting()
    {
        var sut = new PersonsByDeceasedSorter();
        var persons = new List<DeceasedPersonViewModel>
        {
            MakePerson("2010-01-01"),
            MakePerson("1990-01-01"),
            MakePerson("2000-01-01"),
        };

        persons.Sort(sut);

        Assert.Equal("1990-01-01", persons[0].DeceasedStr);
        Assert.Equal("2000-01-01", persons[1].DeceasedStr);
        Assert.Equal("2010-01-01", persons[2].DeceasedStr);
    }
}
