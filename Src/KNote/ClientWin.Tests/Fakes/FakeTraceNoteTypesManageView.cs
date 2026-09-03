using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>IViewManageList&lt;TraceNoteTypeDto&gt; test double for TraceNoteTypesManageCtrl tests.</summary>
internal class FakeTraceNoteTypesManageView : IViewManageList<TraceNoteTypeDto>
{
    public string? LastShownInfo { get; private set; }
    public bool RefreshViewCalled { get; private set; }
    public List<TraceNoteTypeDto> AddedItems { get; } = new();
    public List<TraceNoteTypeDto> UpdatedItems { get; } = new();
    public List<TraceNoteTypeDto> RemovedItems { get; } = new();

    public Control? PanelView() => null;
    public void ConfigureEmbededMode() { }
    public void ConfigureWindowMode() { }
    public void ShowView() { }
    public Result<EControllerResult> ShowModalView() => new(EControllerResult.Executed);
    public void OnClosingView() { }

    public void RefreshView() => RefreshViewCalled = true;
    public void AddItem(TraceNoteTypeDto item) => AddedItems.Add(item);
    public void UpdateItem(TraceNoteTypeDto item) => UpdatedItems.Add(item);
    public void RemoveItem(TraceNoteTypeDto item) => RemovedItems.Add(item);

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        LastShownInfo = info;
        return DialogResult.OK;
    }
}
