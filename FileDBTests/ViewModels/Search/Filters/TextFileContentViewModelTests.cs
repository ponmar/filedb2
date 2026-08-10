using FakeItEasy;
using FileDB.ViewModels.Search.Filters;
using FileDB.Model;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.FilesystemAccess;
using System.IO.Abstractions;
using Xunit;

namespace FileDBTests.ViewModels.Search.Filters;

public class TextFileContentViewModelTests
{
    private static TextFileContentViewModel CreateViewModel(
        IFilesystemAccessProvider? filesystemAccessProvider = null)
    {
        filesystemAccessProvider ??= CreateFilesystemAccessProvider();
        return new TextFileContentViewModel(filesystemAccessProvider);
    }

    private static IFilesystemAccessProvider CreateFilesystemAccessProvider(
        IFilesystemAccess? filesystemAccess = null)
    {
        filesystemAccess ??= A.Fake<IFilesystemAccess>();
        var provider = A.Fake<IFilesystemAccessProvider>();
        A.CallTo(() => provider.FilesystemAccess).Returns(filesystemAccess);
        return provider;
    }

    private static IFilesystemAccess CreateFilesystemAccess(
        Func<string, string>? readAllText = null,
        Func<string, string>? toAbsolutePath = null)
    {
        var filesystemAccess = A.Fake<IFilesystemAccess>();
        var fileSystem = A.Fake<IFileSystem>();
        var fileBase = A.Fake<IFile>();
        A.CallTo(() => filesystemAccess.FileSystem).Returns(fileSystem);
        A.CallTo(() => fileSystem.File).Returns(fileBase);
        A.CallTo(() => filesystemAccess.ToAbsolutePath(A<string>._))
            .ReturnsLazily((string path) => toAbsolutePath != null ? toAbsolutePath(path) : "/collection/" + path);
        A.CallTo(() => fileBase.ReadAllText(A<string>._))
            .ReturnsLazily((string path) => readAllText != null ? readAllText(path) : string.Empty);
        return filesystemAccess;
    }

    [Fact]
    public void ApplyFilter_ReturnsFilesWhoseContentContainsSearchText()
    {
        var matchingFile = SearchFilterViewModelTestHelpers.CreateFile(1, "notes.txt");
        var nonMatchingFile = SearchFilterViewModelTestHelpers.CreateFile(2, "other.txt");

        var filesystemAccess = CreateFilesystemAccess(
            readAllText: path => path.Contains("notes") ? "Hello world match here" : "no hit");
        var provider = CreateFilesystemAccessProvider(filesystemAccess);

        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesByExtension(".txt")).Returns([matchingFile, nonMatchingFile]);

        var viewModel = CreateViewModel(provider);
        viewModel.SearchText = "match";
        viewModel.FileExtensions = ".txt";

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Single(result);
        Assert.Contains(matchingFile, result);
    }

    [Fact]
    public void ApplyFilter_IsCaseInsensitive()
    {
        var file = SearchFilterViewModelTestHelpers.CreateFile(1, "readme.txt");

        var filesystemAccess = CreateFilesystemAccess(readAllText: _ => "lowercase content");
        var provider = CreateFilesystemAccessProvider(filesystemAccess);

        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesByExtension(".txt")).Returns([file]);

        var viewModel = CreateViewModel(provider);
        viewModel.SearchText = "LOWERCASE";
        viewModel.FileExtensions = ".txt";

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Single(result);
        Assert.Contains(file, result);
    }

    [Fact]
    public void ApplyFilter_FiltersOnlyMatchingExtensions()
    {
        var txtFile = SearchFilterViewModelTestHelpers.CreateFile(1, "doc.txt");

        var filesystemAccess = CreateFilesystemAccess(readAllText: _ => "searchterm");
        var provider = CreateFilesystemAccessProvider(filesystemAccess);

        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesByExtension(".txt")).Returns([txtFile]);
        A.CallTo(() => dbAccess.SearchFilesByExtension(".md")).Returns([]);

        var viewModel = CreateViewModel(provider);
        viewModel.SearchText = "searchterm";
        viewModel.FileExtensions = ".txt .md";

        viewModel.ApplyFilter(dbAccess).ToList();

        A.CallTo(() => dbAccess.SearchFilesByExtension(".txt")).MustHaveHappenedOnceExactly();
        A.CallTo(() => dbAccess.SearchFilesByExtension(".md")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void ApplyFilter_NormalisesExtensionWithoutLeadingDot()
    {
        var file = SearchFilterViewModelTestHelpers.CreateFile(1, "log.txt");

        var filesystemAccess = CreateFilesystemAccess(readAllText: _ => "found");
        var provider = CreateFilesystemAccessProvider(filesystemAccess);

        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesByExtension(".txt")).Returns([file]);

        var viewModel = CreateViewModel(provider);
        viewModel.SearchText = "found";
        viewModel.FileExtensions = "txt"; // no leading dot

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Single(result);
        A.CallTo(() => dbAccess.SearchFilesByExtension(".txt")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void ApplyFilter_CaseSensitive_DoesNotMatchWrongCase()
    {
        var file = SearchFilterViewModelTestHelpers.CreateFile(1, "doc.txt");

        var filesystemAccess = CreateFilesystemAccess(readAllText: _ => "lowercase content only");
        var provider = CreateFilesystemAccessProvider(filesystemAccess);

        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesByExtension(".txt")).Returns([file]);

        var viewModel = CreateViewModel(provider);
        viewModel.SearchText = "LOWERCASE";
        viewModel.FileExtensions = ".txt";
        viewModel.CaseSensitive = true;

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void ApplyFilter_CaseSensitive_MatchesExactCase()
    {
        var file = SearchFilterViewModelTestHelpers.CreateFile(1, "doc.txt");

        var filesystemAccess = CreateFilesystemAccess(readAllText: _ => "ExactCase content");
        var provider = CreateFilesystemAccessProvider(filesystemAccess);

        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesByExtension(".txt")).Returns([file]);

        var viewModel = CreateViewModel(provider);
        viewModel.SearchText = "ExactCase";
        viewModel.FileExtensions = ".txt";
        viewModel.CaseSensitive = true;

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Single(result);
        Assert.Contains(file, result);
    }

    [Fact]
    public void ApplyFilter_SkipsUnreadableFiles()
    {
        var unreadableFile = SearchFilterViewModelTestHelpers.CreateFile(1, "locked.txt");
        var readableFile = SearchFilterViewModelTestHelpers.CreateFile(2, "readable.txt");

        var filesystemAccess = CreateFilesystemAccess(
            readAllText: path =>
            {
                if (path.Contains("locked")) throw new IOException("Access denied");
                return "searchterm";
            });
        var provider = CreateFilesystemAccessProvider(filesystemAccess);

        var dbAccess = A.Fake<IDatabaseAccess>();
        A.CallTo(() => dbAccess.SearchFilesByExtension(".txt")).Returns([unreadableFile, readableFile]);

        var viewModel = CreateViewModel(provider);
        viewModel.SearchText = "searchterm";
        viewModel.FileExtensions = ".txt";

        var result = viewModel.ApplyFilter(dbAccess).ToList();

        Assert.Single(result);
        Assert.Contains(readableFile, result);
        Assert.DoesNotContain(unreadableFile, result);
    }
}

