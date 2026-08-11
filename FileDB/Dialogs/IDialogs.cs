using FileDB.ViewModels.Search;
using FileDBInterface.Model;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FileDB.Dialogs;

public interface IDialogs
{
    Task ShowInfoDialogAsync(string message);
    Task ShowWarningDialogAsync(string message);
    Task ShowErrorDialogAsync(string message);
    Task ShowErrorDialogAsync(string message, Exception e);
    Task ShowErrorDialogAsync(IEnumerable<string> messages);
    Task ShowErrorDialogAsync(ValidationResult validationResult);
    Task<bool> ShowConfirmDialogAsync(string question);
    Task ShowProgressDialogAsync(Action<IProgress<string>> work);
    Task ShowProgressDialogAsync(Action<IProgress<string>, CancellationToken> work);
    Task<string?> ShowBrowseExistingDirectoryDialogAsync(string title);
    Task<string?> ShowBrowseExistingDirectoryDialogAsync(string title, string initialDirectory);
    Task<string?> ShowBrowseExistingSubDirectoryDialogAsync(string title, string rootDirectory);
    Task<string?> ShowSaveFileDialogAsync(string title, string suggestedFileName, string extension, string extensionDescription);
    Task<PersonModel?> ShowAddPersonDialogAsync(int? personId = null, string? personName = null);
    Task<LocationModel?> ShowAddLocationDialogAsync(int? locationId = null, string? locationName = null);
    Task<TagModel?> ShowAddTagDialogAsync(int? tagId = null, string? tagName = null);
    Task ShowExportSearchResultDialogAsync(SearchResult searchResult);
    void ShowPresentationWindow(string title);
}
