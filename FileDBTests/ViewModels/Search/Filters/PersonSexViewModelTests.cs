using FakeItEasy;
using System.Linq;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.ViewModels.Search.Filters;

public class PersonSexViewModelTests
{
    [Fact]
    public void Constructor_DefaultsToFirstSortedSex()
    {
        var viewModel = new PersonSexViewModel();

        Assert.Equal(PersonSexViewModel.PersonSexValues.First(), viewModel.SelectedPersonSex);
    }

    [Fact]
    public void ApplyFilter_UsesSelectedSex()
    {
        var files = new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(1, "person.jpg"),
        };
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesBySex(Sex.Female)).Returns(files);
        var viewModel = new PersonSexViewModel { SelectedPersonSex = Sex.Female };

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(files, result);
    }
}
