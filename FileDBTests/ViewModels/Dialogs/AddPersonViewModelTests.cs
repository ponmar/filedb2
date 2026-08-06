using FakeItEasy;
using FileDB.Dialogs;
using FileDB.Lang;
using FileDB.Model;
using FileDB.Services;
using FileDB.ViewModels.Search;
using FileDB.ViewModels.Dialogs;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.FilesystemAccess;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.ViewModels.Dialogs;

public class AddPersonViewModelTests
{
    private readonly IDatabaseAccessProvider dbAccessProvider = A.Fake<IDatabaseAccessProvider>();
    private readonly IDatabaseAccess dbAccess = A.Fake<IDatabaseAccess>();
    private readonly IDialogs dialogs = A.Fake<IDialogs>();
    private readonly IFileSelector fileSelector = A.Fake<IFileSelector>();
    private readonly IFilesystemAccessProvider filesystemAccessProvider = A.Fake<IFilesystemAccessProvider>();
    private readonly IFilesystemAccess filesystemAccess = A.Fake<IFilesystemAccess>();
    private readonly IImageLoader imageLoader = A.Fake<IImageLoader>();
    private readonly IClipboardService clipboardService = A.Fake<IClipboardService>();

    public AddPersonViewModelTests()
    {
        A.CallTo(() => dbAccessProvider.DbAccess).Returns(dbAccess);
        A.CallTo(() => filesystemAccessProvider.FilesystemAccess).Returns(filesystemAccess);
    }

    [Fact]
    public void Constructor_NewPerson_UsesAddTitle()
    {
        A.CallTo(() => fileSelector.SelectedFile).Returns(null);

        var viewModel = new AddPersonViewModel(dbAccessProvider, dialogs, fileSelector, filesystemAccessProvider, imageLoader, clipboardService);

        Assert.Equal(Strings.AddPersonAddTitle, viewModel.Title);
        Assert.Null(viewModel.AffectedPerson);
    }

    [Fact]
    public void Constructor_EditPerson_LoadsExistingValues()
    {
        var profileFile = new FileModel { Id = 8, Path = "photo.jpg" };
        A.CallTo(() => dbAccess.GetPersonById(5)).Returns(new PersonModel
        {
            Id = 5,
            ShortName = "Alice",
            FullName = "Alice Smith",
            Description = "Desc",
            DateOfBirth = "2000-01-01",
            Deceased = "2020-01-01",
            Sex = Sex.Female,
            ProfileFileId = 8,
        });
        A.CallTo(() => dbAccess.GetFileById(8)).Returns(profileFile);
        A.CallTo(() => fileSelector.SelectedFile).Returns(null);

        var viewModel = new AddPersonViewModel(dbAccessProvider, dialogs, fileSelector, filesystemAccessProvider, imageLoader, clipboardService, 5);

        Assert.Equal(Strings.AddPersonEditTitle, viewModel.Title);
        Assert.Equal("Alice", viewModel.ShortName);
        Assert.Equal("Alice Smith", viewModel.FullName);
        Assert.Equal("Desc", viewModel.Description);
        Assert.Equal("2000-01-01", viewModel.DateOfBirth);
        Assert.Equal("2020-01-01", viewModel.Deceased);
        Assert.Equal(Sex.Female.ToString(), viewModel.SexSelection);
        Assert.Equal(profileFile, viewModel.ProfilePictureFile);
    }

    [Fact]
    public void ProfilePictureFile_Changed_LoadsImageAndSetsRotation()
    {
        A.CallTo(() => fileSelector.SelectedFile).Returns(null);
        A.CallTo(() => filesystemAccess.ToAbsolutePath("photo.jpg")).Returns("/abs/photo.jpg");

        var viewModel = new AddPersonViewModel(dbAccessProvider, dialogs, fileSelector, filesystemAccessProvider, imageLoader, clipboardService);
        var file = new FileModel { Id = 8, Path = "photo.jpg", Orientation = 8 };

        viewModel.ProfilePictureFile = file;

        A.CallTo(() => imageLoader.LoadImage("/abs/photo.jpg")).MustHaveHappenedOnceExactly();
        Assert.Equal(-90, viewModel.ProfilePictureRotation);
    }

    [Fact]
    public async Task SaveCommand_NewPerson_InsertsPerson()
    {
        A.CallTo(() => fileSelector.SelectedFile).Returns(null);
        A.CallTo(() => dbAccess.GetPersons()).Returns([]);
        A.CallTo(() => dbAccess.InsertPerson(A<PersonModel>._)).Returns(5);
        A.CallTo(() => dbAccess.GetPersonById(5)).Returns(new PersonModel { Id = 5, ShortName = "Alice", FullName = "Alice Smith" });

        var viewModel = new AddPersonViewModel(dbAccessProvider, dialogs, fileSelector, filesystemAccessProvider, imageLoader, clipboardService)
        {
            ShortName = "Alice",
            FullName = "Alice Smith",
        };

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(5, viewModel.AffectedPerson!.Id);
        A.CallTo(() => dbAccess.InsertPerson(A<PersonModel>.That.Matches(x => x.ShortName == "Alice" && x.FullName == "Alice Smith"))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SaveCommand_DuplicatePerson_Declined_DoesNotInsert()
    {
        A.CallTo(() => fileSelector.SelectedFile).Returns(null);
        A.CallTo(() => dbAccess.GetPersons()).Returns([new PersonModel { Id = 1, ShortName = "Alice", FullName = "Alice Smith" }]);
        A.CallTo(() => dialogs.ShowConfirmDialogAsync(A<string>._)).Returns(false);

        var viewModel = new AddPersonViewModel(dbAccessProvider, dialogs, fileSelector, filesystemAccessProvider, imageLoader, clipboardService)
        {
            ShortName = "Alice",
            FullName = "Alice Smith",
        };

        await viewModel.SaveCommand.ExecuteAsync(null);

        A.CallTo(() => dbAccess.InsertPerson(A<PersonModel>._)).MustNotHaveHappened();
        Assert.Null(viewModel.AffectedPerson);
    }

    [Fact]
    public async Task SaveCommand_EditPerson_UpdatesPerson()
    {
        A.CallTo(() => fileSelector.SelectedFile).Returns(null);
        A.CallTo(() => dbAccess.GetPersonById(5)).Returns(new PersonModel { Id = 5, ShortName = "Alice", FullName = "Alice Smith" });

        var viewModel = new AddPersonViewModel(dbAccessProvider, dialogs, fileSelector, filesystemAccessProvider, imageLoader, clipboardService, 5)
        {
            ShortName = "Alice",
            FullName = "Alice Smith",
        };

        await viewModel.SaveCommand.ExecuteAsync(null);

        A.CallTo(() => dbAccess.UpdatePerson(A<PersonModel>.That.Matches(x => x.Id == 5))).MustHaveHappenedOnceExactly();
        Assert.NotNull(viewModel.AffectedPerson);
    }

    [Fact]
    public void SetProfilePictureFromSearchResultCommand_UsesSelectedFile()
    {
        var selectedFile = new FileModel { Id = 7, Path = "photo.jpg" };
        A.CallTo(() => fileSelector.SelectedFile).Returns(selectedFile);
        var viewModel = new AddPersonViewModel(dbAccessProvider, dialogs, fileSelector, filesystemAccessProvider, imageLoader, clipboardService);

        viewModel.SetProfilePictureFromSearchResultCommand.Execute(null);

        Assert.Same(selectedFile, viewModel.ProfilePictureFile);
    }

    [Fact]
    public void CopyProfilePictureIdCommand_CopiesFileList()
    {
        var selectedFile = new FileModel { Id = 7, Path = "photo.jpg" };
        A.CallTo(() => fileSelector.SelectedFile).Returns(selectedFile);
        var viewModel = new AddPersonViewModel(dbAccessProvider, dialogs, fileSelector, filesystemAccessProvider, imageLoader, clipboardService)
        {
            ProfilePictureFile = selectedFile,
        };

        viewModel.CopyProfilePictureIdCommand.Execute(null);

        A.CallTo(() => clipboardService.SetTextAsync(A<string>.That.Contains("7"))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void ClearProfilePictureCommand_ClearsFile()
    {
        var selectedFile = new FileModel { Id = 7, Path = "photo.jpg" };
        A.CallTo(() => fileSelector.SelectedFile).Returns(selectedFile);
        var viewModel = new AddPersonViewModel(dbAccessProvider, dialogs, fileSelector, filesystemAccessProvider, imageLoader, clipboardService)
        {
            ProfilePictureFile = selectedFile,
        };

        viewModel.ClearProfilePictureCommand.Execute(null);

        Assert.Null(viewModel.ProfilePictureFile);
    }

    [Fact]
    public void CanSetProfilePicture_ReflectsFileSelectorState()
    {
        A.CallTo(() => fileSelector.SelectedFile).Returns(new FileModel { Id = 1, Path = "photo.jpg" });
        var viewModel = new AddPersonViewModel(dbAccessProvider, dialogs, fileSelector, filesystemAccessProvider, imageLoader, clipboardService);

        Assert.True(viewModel.CanSetProfilePicture);
    }
}
