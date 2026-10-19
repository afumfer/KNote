using KNote.Client.AppStoreService;
using KNote.Client.Shared;
using KNote.Model.Dto;
using Radzen;

namespace KNote.Client.Pages.Folders;

/// <summary>
/// The folder use cases shared by the folders management (Admin panel) and the tree panel: each one returns whether
/// the folders changed, so the page reads its tree again. Who can use them is checked by the page.
/// </summary>
public static class FolderDialogs
{
    /// <summary>A new folder, inside the selected one (AppState.SelectedFolder).</summary>
    public static async Task<bool> NewAsync(DialogService dialogService)
    {
        object? result = await dialogService.OpenAsync<FolderNew>("New folder",
            new Dictionary<string, object?>(), KntDialogs.Framed(KntDialogs.Medium()));
        return result != null;
    }

    public static async Task<bool> EditAsync(DialogService dialogService, FolderDto folder)
    {
        object? result = await dialogService.OpenAsync<FolderEdit>("Edit folder",
            new Dictionary<string, object?> { { "FolderId", folder.FolderId.ToString() } }, KntDialogs.Framed(KntDialogs.Medium()));
        return result != null;
    }

    /// <summary>Asks and deletes the folder.</summary>
    public static async Task<bool> DeleteAsync(DialogService dialogService, IStore store, FolderDto folder)
    {
        var confirmed = await dialogService.Confirm(
            $"Are you sure you want delete folder - {folder.Name}?",
            "Delete folder",
            new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });
        if (confirmed != true)
            return false;

        var result = await store.Folders.DeleteAsync(folder.FolderId);
        return result.IsValid;
    }
}
