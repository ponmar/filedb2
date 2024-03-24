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
    /*
    string? BrowseExistingFileDialog(string initialDirectory, string filter);
    string? BrowseExistingDirectory(string initialDirectory, string title);
    string? SelectNewFileDialog(string title, string fileExtension, string filter);
    */
    PersonModel? ShowAddPersonDialog(int? personId = null);
    LocationModel? ShowAddLocationDialog(int? locationId = null);
    TagModel? ShowAddTagDialog(int? tagId = null);
    //string? ShowBrowseDirectoriesDialog();
    //void ShowExportDialog(SearchResult searchResult);
}
