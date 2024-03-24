using FileDBAvalonia.ViewModels.Search;
using FileDBShared.Model;
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
    PersonModel? ShowAddPersonDialog(int? personId = null);
    LocationModel? ShowAddLocationDialog(int? locationId = null);
    TagModel? ShowAddTagDialog(int? tagId = null);
    //string? ShowBrowseDirectoriesDialog();
    void ShowExportSearchResultDialog(SearchResult searchResult);
}
