using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaFilesHelper2.Dialogs;

public interface IDialogs
{
    Task ShowInfoDialogAsync(string message);
    Task ShowWarningDialogAsync(string message);
    Task ShowErrorDialogAsync(string message);
    Task ShowErrorDialogAsync(string message, Exception e);
    Task ShowErrorDialogAsync(IEnumerable<string> messages);
    Task<bool> ShowConfirmDialogAsync(string question);
    Task<string?> ShowBrowseExistingDirectoryDialogAsync(string title);
    Task<string?> ShowBrowseExistingDirectoryDialogAsync(string title, string initialDirectory);
}
