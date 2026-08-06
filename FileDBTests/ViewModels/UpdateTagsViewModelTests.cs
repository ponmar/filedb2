using FakeItEasy;
using FileDB.Dialogs;
using FileDB.Model;
using FileDB.ViewModels;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.ViewModels;

public class UpdateTagsViewModelTests
{
    private readonly IConfigProvider configProvider = A.Fake<IConfigProvider>();
    private readonly IDatabaseAccessProvider dbAccessProvider = A.Fake<IDatabaseAccessProvider>();
    private readonly IDatabaseAccess dbAccess = A.Fake<IDatabaseAccess>();
    private readonly IDialogs dialogs = A.Fake<IDialogs>();
    private readonly ITagsRepository tagsRepository = A.Fake<ITagsRepository>();

    public UpdateTagsViewModelTests()
    {
        A.CallTo(() => dbAccessProvider.DbAccess).Returns(dbAccess);
        A.CallTo(() => configProvider.Config).Returns(new ConfigBuilder { ReadOnly = false }.Build());
    }

    [Fact]
    public void Constructor_LoadsTags()
    {
        A.CallTo(() => tagsRepository.Tags).Returns([
            new TagModel { Id = 1, Name = "Favorites" },
            new TagModel { Id = 2, Name = "Family" },
        ]);

        var viewModel = new UpdateTagsViewModel(configProvider, dbAccessProvider, dialogs, tagsRepository);

        Assert.Equal(2, viewModel.Tags.Count);
        Assert.False(viewModel.ReadOnly);
    }

    [Fact]
    public void AddTagCommand_UsesFilterText()
    {
        A.CallTo(() => tagsRepository.Tags).Returns([]);

        var viewModel = new UpdateTagsViewModel(configProvider, dbAccessProvider, dialogs, tagsRepository)
        {
            FilterText = "Favorites",
        };

        viewModel.AddTagCommand.Execute(null);

        A.CallTo(() => dialogs.ShowAddTagDialogAsync(null, "Favorites")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RemoveTagCommand_DeletesSelectedTag()
    {
        A.CallTo(() => tagsRepository.Tags).Returns([
            new TagModel { Id = 1, Name = "Favorites" },
        ]);
        A.CallTo(() => dialogs.ShowConfirmDialogAsync(A<string>._)).Returns(true);
        A.CallTo(() => dbAccess.SearchFilesWithTags(A<IEnumerable<int>>._)).Returns([]);
        var recorder = new SingleEventRecorder<TagEdited>();

        var viewModel = new UpdateTagsViewModel(configProvider, dbAccessProvider, dialogs, tagsRepository)
        {
            SelectedTag = new TagModel { Id = 1, Name = "Favorites" },
        };

        await viewModel.RemoveTagCommand.ExecuteAsync(null);

        A.CallTo(() => dbAccess.DeleteTag(1)).MustHaveHappenedOnceExactly();
        recorder.AssertEventRecorded();
    }

    [Fact]
    public void EditTagCommand_UsesSelectedTag()
    {
        A.CallTo(() => tagsRepository.Tags).Returns([
            new TagModel { Id = 1, Name = "Favorites" },
        ]);

        var viewModel = new UpdateTagsViewModel(configProvider, dbAccessProvider, dialogs, tagsRepository)
        {
            SelectedTag = new TagModel { Id = 1, Name = "Favorites" },
        };

        viewModel.EditTagCommand.Execute(null);

        A.CallTo(() => dialogs.ShowAddTagDialogAsync(1, null)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void TagSelectionCommand_SetsSelection()
    {
        A.CallTo(() => tagsRepository.Tags).Returns([]);
        var viewModel = new UpdateTagsViewModel(configProvider, dbAccessProvider, dialogs, tagsRepository);
        var selected = new TagModel { Id = 7, Name = "Seven" };

        viewModel.TagSelectionCommand.Execute(selected);

        Assert.Same(selected, viewModel.SelectedTag);
    }
}
