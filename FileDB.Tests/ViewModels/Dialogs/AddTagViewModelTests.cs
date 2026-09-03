using FakeItEasy;
using FileDB.Dialogs;
using FileDB.Lang;
using FileDB.Model;
using FileDB.ViewModels.Dialogs;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using Xunit;

namespace FileDB.Tests.ViewModels.Dialogs;

public class AddTagViewModelTests
{
    private readonly IDatabaseAccessProvider dbAccessProvider = A.Fake<IDatabaseAccessProvider>();
    private readonly IDatabaseAccess dbAccess = A.Fake<IDatabaseAccess>();
    private readonly IDialogs dialogs = A.Fake<IDialogs>();

    public AddTagViewModelTests()
    {
        A.CallTo(() => dbAccessProvider.DbAccess).Returns(dbAccess);
    }

    [Fact]
    public void Constructor_NewTag_UsesAddTitle()
    {
        var viewModel = new AddTagViewModel(dbAccessProvider, dialogs);

        Assert.Equal(Strings.AddTagAddTitle, viewModel.Title);
        Assert.Null(viewModel.AffectedTag);
    }

    [Fact]
    public void Constructor_EditTag_LoadsExistingValues()
    {
        A.CallTo(() => dbAccess.GetTagById(4)).Returns(new TagModel { Id = 4, Name = "Favorites" });

        var viewModel = new AddTagViewModel(dbAccessProvider, dialogs, 4);

        Assert.Equal(Strings.AddTagEditTitle, viewModel.Title);
        Assert.Equal("Favorites", viewModel.Name);
    }

    [Fact]
    public async Task SaveCommand_NewTag_InsertsTag()
    {
        A.CallTo(() => dbAccess.GetTags()).Returns([]);
        A.CallTo(() => dbAccess.InsertTag(A<TagModel>._)).Returns(9);
        A.CallTo(() => dbAccess.GetTagById(9)).Returns(new TagModel { Id = 9, Name = "Favorites" });

        var viewModel = new AddTagViewModel(dbAccessProvider, dialogs)
        {
            Name = "Favorites",
        };

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(9, viewModel.AffectedTag!.Id);
        A.CallTo(() => dbAccess.InsertTag(A<TagModel>.That.Matches(x => x.Name == "Favorites"))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SaveCommand_EditTag_UpdatesTag()
    {
        A.CallTo(() => dbAccess.GetTagById(4)).Returns(new TagModel { Id = 4, Name = "Favorites" });

        var viewModel = new AddTagViewModel(dbAccessProvider, dialogs, 4)
        {
            Name = "Favorites updated",
        };

        await viewModel.SaveCommand.ExecuteAsync(null);

        A.CallTo(() => dbAccess.UpdateTag(A<TagModel>.That.Matches(x => x.Id == 4 && x.Name == "Favorites updated"))).MustHaveHappenedOnceExactly();
        Assert.NotNull(viewModel.AffectedTag);
    }

    [Fact]
    public async Task SaveCommand_DuplicateTag_ShowsError()
    {
        A.CallTo(() => dbAccess.GetTags()).Returns([new TagModel { Id = 1, Name = "Favorites" }]);

        var viewModel = new AddTagViewModel(dbAccessProvider, dialogs)
        {
            Name = "Favorites",
        };

        await viewModel.SaveCommand.ExecuteAsync(null);

        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<string>.That.Contains("Favorites"))).MustHaveHappenedOnceExactly();
        A.CallTo(() => dbAccess.InsertTag(A<TagModel>._)).MustNotHaveHappened();
    }
}
