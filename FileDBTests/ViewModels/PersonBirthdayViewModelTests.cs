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

    [Fact]
    public void Constructor_WhenBirthdayIsFarAway_LeavesDaysLeftTextEmpty()
    {
        var person = new PersonModel
        {
            Id = 1,
            ShortName = "Alice",
            FullName = "Alice Smith",
            DateOfBirth = DateTime.Today.AddDays(30).AddYears(-30).ToString("yyyy-MM-dd"),
        };

        var viewModel = new PersonBirthdayViewModel(CreateCriteriaViewModel(), person, "/collection/photo.jpg");

        Assert.Equal(string.Empty, viewModel.DaysLeftStr);
        Assert.Null(viewModel.ProfilePictureAbsPath);
    }

    [Fact]
    public void AddPersonSearchFilterCommand_CallsCriteria()
    {
        var criteriaViewModel = A.Fake<ICriteriaViewModel>();
        var person = new PersonModel
        {
            Id = 1,
            ShortName = "Alice",
            FullName = "Alice Smith",
            DateOfBirth = DateTime.Today.AddYears(-30).ToString("yyyy-MM-dd"),
        };
        var viewModel = new PersonBirthdayViewModel(criteriaViewModel, person, "/collection/photo.jpg");

        viewModel.AddPersonSearchFilterCommand.Execute(null);

        A.CallTo(() => criteriaViewModel.AddPersonSearchFilter(person)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void SearchForBirthdayCommand_CallsCriteria()
    {
        var criteriaViewModel = A.Fake<ICriteriaViewModel>();
        var birthday = DateTime.Today.AddDays(1).AddYears(-30);
        var person = new PersonModel
        {
            Id = 1,
            ShortName = "Alice",
            FullName = "Alice Smith",
            DateOfBirth = birthday.ToString("yyyy-MM-dd"),
        };
        var viewModel = new PersonBirthdayViewModel(criteriaViewModel, person, "/collection/photo.jpg");

        viewModel.SearchForBirthdayCommand.Execute(null);

        A.CallTo(() => criteriaViewModel.SearchForAnnualDateAsync(birthday.Month, birthday.Day)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void AddBirthdayDateSearchFilterCommand_CallsCriteria()
    {
        var criteriaViewModel = A.Fake<ICriteriaViewModel>();
        var birthday = DateTime.Today.AddDays(1).AddYears(-30);
        var person = new PersonModel
        {
            Id = 1,
            ShortName = "Alice",
            FullName = "Alice Smith",
            DateOfBirth = birthday.ToString("yyyy-MM-dd"),
        };
        var viewModel = new PersonBirthdayViewModel(criteriaViewModel, person, "/collection/photo.jpg");

        viewModel.AddBirthdayDateSearchFilterCommand.Execute(null);

        A.CallTo(() => criteriaViewModel.AddAnnualDateSearchFilter(birthday.Month, birthday.Day)).MustHaveHappenedOnceExactly();
    }
}
