using FileDBAvalonia.ViewModels.Search;
using FileDBInterface.Model;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileDBAvalonia.Dialogs;

public interface IDialogs
{
    Task ShowInfoDialogAsync(string message);
    Task ShowWarningDialogAsync(string message);
    Task ShowErrorDialogAsync(string message);
    Task ShowErrorDialogAsync(string message, Exception e);
    Task ShowErrorDialogAsync(IEnumerable<string> messages);
    Task ShowErrorDialogAsync(ValidationResult validationResult);
    Task<bool> ShowConfirmDialogAsync(string question);
    void ShowProgressDialog(Action<IProgress<string>> work);
    Task<string?> ShowBrowseExistingDirectoryDialogAsync(string title);
    Task<string?> ShowBrowseExistingDirectoryDialogAsync(string title, string initialDirectory);
    Task<PersonModel?> ShowAddPersonDialogAsync(int? personId = null, string? personName = null);
    Task<LocationModel?> ShowAddLocationDialogAsync(int? locationId = null, string? locationName = null);
    Task<TagModel?> ShowAddTagDialogAsync(int? tagId = null, string? tagName = null);
    Task<string?> ShowBrowseDirectoriesDialogAsync();
    void ShowExportSearchResultDialog(SearchResult searchResult);
    void ShowPresentationWindow(string title);
}
