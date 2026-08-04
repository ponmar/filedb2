using FakeItEasy;
using FileDB.Lang;
using FileDB.ViewModels;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.ViewModels;

public class PersonBirthdayViewModelTests
{
    private static ICriteriaViewModel CreateCriteriaViewModel() => A.Fake<ICriteriaViewModel>();

    [Fact]
    public void Constructor_WhenBirthdayIsToday_UsesLocalizedText()
    {
        var person = new PersonModel
        {
            Id = 1,
            ShortName = "Alice",
            FullName = "Alice Smith",
            DateOfBirth = DateTime.Today.AddYears(-30).ToString("yyyy-MM-dd"),
        };

        var viewModel = new PersonBirthdayViewModel(CreateCriteriaViewModel(), person, "/collection/photo.jpg");

        Assert.Equal(string.Format(Strings.PersonBirthdayViewModelTurnedToday, viewModel.Age), viewModel.DaysLeftStr);
    }

    [Fact]
    public void Constructor_WhenBirthdayIsTomorrow_UsesLocalizedText()
    {
        var person = new PersonModel
        {
            Id = 1,
            ShortName = "Alice",
            FullName = "Alice Smith",
            DateOfBirth = DateTime.Today.AddDays(1).AddYears(-30).ToString("yyyy-MM-dd"),
        };

        var viewModel = new PersonBirthdayViewModel(CreateCriteriaViewModel(), person, "/collection/photo.jpg");

        Assert.Equal(string.Format(Strings.PersonBirthdayViewModelTurnsTomorrow, viewModel.Age + 1), viewModel.DaysLeftStr);
    }

    [Fact]
    public void Constructor_WhenBirthdayIsWithinTwoWeeks_UsesLocalizedText()
    {
        var person = new PersonModel
        {
            Id = 1,
            ShortName = "Alice",
            FullName = "Alice Smith",
            DateOfBirth = DateTime.Today.AddDays(7).AddYears(-30).ToString("yyyy-MM-dd"),
        };

        var viewModel = new PersonBirthdayViewModel(CreateCriteriaViewModel(), person, "/collection/photo.jpg");

        Assert.Equal(string.Format(Strings.PersonBirthdayViewModelTurnsInDays, viewModel.Age + 1, viewModel.DaysLeft), viewModel.DaysLeftStr);
    }
}
