using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>IViewManageList&lt;NoteTypeDto&gt; test double for NoteTypesManageCtrl tests.</summary>
internal class FakeNoteTypesManageView : IViewManageList<NoteTypeDto>
{
    public string? LastShownInfo { get; private set; }
    public bool RefreshViewCalled { get; private set; }
    public List<NoteTypeDto> AddedItems { get; } = new();
    public List<NoteTypeDto> UpdatedItems { get; } = new();
    public List<NoteTypeDto> RemovedItems { get; } = new();

    public Control? PanelView() => null;
    public void ConfigureEmbededMode() { }
    public void ConfigureWindowMode() { }
    public void ShowView() { }
    public Result<EControllerResult> ShowModalView() => new(EControllerResult.Executed);
    public void OnClosingView() { }

    public void RefreshView() => RefreshViewCalled = true;
    public void AddItem(NoteTypeDto item) => AddedItems.Add(item);
    public void UpdateItem(NoteTypeDto item) => UpdatedItems.Add(item);
    public void RemoveItem(NoteTypeDto item) => RemovedItems.Add(item);

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        LastShownInfo = info;
        return DialogResult.OK;
    }
}
