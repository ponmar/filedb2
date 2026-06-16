using FakeItEasy;
using System.Collections.Generic;
using System.Linq;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDBTests.ViewModels.Search.Filters;

public class PersonProfileFilesViewModelTests
{
    [Fact]
    public void ApplyFilter_ReturnsProfileFiles()
    {
        var persons = new[]
        {
            SearchFilterViewModelTestHelpers.CreatePerson(1, "Alice", profileFileId: 10),
            SearchFilterViewModelTestHelpers.CreatePerson(2, "Bob"),
            SearchFilterViewModelTestHelpers.CreatePerson(3, "Charlie", profileFileId: 20),
        };
        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesFromIds(A<IEnumerable<int>>._)).Returns(new[]
        {
            SearchFilterViewModelTestHelpers.CreateFile(10, "a.jpg"),
            SearchFilterViewModelTestHelpers.CreateFile(20, "b.jpg"),
        });
        var viewModel = new PersonProfileFilesViewModel(SearchFilterViewModelTestHelpers.CreatePersonsRepository(persons));

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Equal(2, result.Count);
        A.CallTo(() => dbAccess.SearchFilesFromIds(A<IEnumerable<int>>.That.Matches(ids => ids.SequenceEqual(new[] { 10, 20 })))).MustHaveHappened();
    }
}
