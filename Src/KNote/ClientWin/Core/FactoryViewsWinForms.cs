using KNote.ClientWin.Controllers;
using KNote.ClientWin.Views;
using KNote.Model;
using KNote.Model.Dto;
using System.ComponentModel;

namespace KNote.ClientWin.Core;

public class FactoryViewsWinForms : IFactoryViews
{
    public ViewFactoryRegistry Registry { get; } = new();

    public FactoryViewsWinForms()
    {
        Registry.Register<MonitorCtrl, IViewBase>(c => new MonitorForm(c));
        Registry.Register<KntScriptConsoleCtrl, IViewBase>(c => new KntScriptConsoleForm(c));
        Registry.Register<FoldersSelectorCtrl, IViewSelector<FolderWithServiceRef>>(c => new FoldersSelectorForm(c));
        Registry.Register<NotesSelectorCtrl, IViewSelector<NoteMinimalDto>>(c => new NotesSelectorForm(c));
        Registry.Register<KNoteManagmentCtrl, IViewKNoteManagment>(c => new KNoteManagmentForm(c));
        Registry.Register<NoteEditorCtrl, IViewEditorEmbeddable<NoteExtendedDto>>(c => new NoteEditorForm(c));
        Registry.Register<PostItEditorCtrl, IViewPostIt<NoteDto>>(c => new PostItEditorForm(c));
        Registry.Register<PostItPropertiesCtrl, IViewPostIt<WindowDto>>(c => new PostItPropertiesForm(c));
        Registry.Register<FolderEditorCtrl, IViewEditor<FolderDto>>(c => new FolderEditorForm(c));
        Registry.Register<RepositoryEditorCtrl, IViewEditor<RepositoryRef>>(c => new RepositoryEditorForm(c));
        Registry.Register<KNoteManagmentCtrl, IViewBase>(c => new NotifyForm(c), key: "Notify");
        Registry.Register<KNoteManagmentCtrl, IViewBase>(c => new KNoteAboutForm(c), key: "About");
        Registry.Register<MessageEditorCtrl, IViewEditor<KMessageDto>>(c => new MessageEditorForm(c));
        Registry.Register<ResourceEditorCtrl, IViewEditor<ResourceDto>>(c => new ResourceEditorForm(c));
        Registry.Register<AttributeEditorCtrl, IViewEditor<KAttributeDto>>(c => new AttributeEditorForm(c));
        Registry.Register<TaskEditorCtrl, IViewEditor<NoteTaskDto>>(c => new TaskEditorForm(c));
        Registry.Register<NoteTypesSelectorCtrl, IViewSelector<NoteTypeDto>>(c => new NoteTypesSelectorForm(c));
        Registry.Register<NoteAttributeEditorCtrl, IViewEditor<NoteKAttributeDto>>(c => new NoteAttributeEditorForm(c));
        Registry.Register<FiltersSelectorCtrl, IViewSelector<SelectedNotesInServiceRef>>(c => new FiltersSelectorForm(c));
        Registry.Register<OptionsEditorCtrl, IViewEditor<AppConfig>>(c => new OptionsEditorForm(c));
        Registry.Register<KntChatGPTCtrl, IViewBase>(c => new KntChatGPTForm(c));
        Registry.Register<KntChatCtrl, IViewChat>(c => new KntChatForm(c));
        Registry.Register<KntServerCOMCtrl, IViewServerCOM>(c => new KntServerCOMForm(c));
        Registry.Register<KntLabCtrl, IViewBase>(c => new KntLabForm(c));
        Registry.Register<HeavyProcessCtrl, IViewHeavyProcess>(c => new HeavyProcessForm(c));
    }

    public IViewBase View(MonitorCtrl controller) => Registry.Resolve<MonitorCtrl, IViewBase>(controller);

    public IViewBase View(KntScriptConsoleCtrl controller) => Registry.Resolve<KntScriptConsoleCtrl, IViewBase>(controller);

    public IViewSelector<FolderWithServiceRef> View(FoldersSelectorCtrl controller) => Registry.Resolve<FoldersSelectorCtrl, IViewSelector<FolderWithServiceRef>>(controller);

    public IViewSelector<NoteMinimalDto> View(NotesSelectorCtrl controller) => Registry.Resolve<NotesSelectorCtrl, IViewSelector<NoteMinimalDto>>(controller);

    public IViewKNoteManagment View(KNoteManagmentCtrl controller) => Registry.Resolve<KNoteManagmentCtrl, IViewKNoteManagment>(controller);

    public IViewEditorEmbeddable<NoteExtendedDto> View(NoteEditorCtrl controller) => Registry.Resolve<NoteEditorCtrl, IViewEditorEmbeddable<NoteExtendedDto>>(controller);

    public IViewPostIt<NoteDto> View(PostItEditorCtrl controller) => Registry.Resolve<PostItEditorCtrl, IViewPostIt<NoteDto>>(controller);

    public IViewPostIt<WindowDto> View(PostItPropertiesCtrl controller) => Registry.Resolve<PostItPropertiesCtrl, IViewPostIt<WindowDto>>(controller);

    public IViewEditor<FolderDto> View(FolderEditorCtrl controller) => Registry.Resolve<FolderEditorCtrl, IViewEditor<FolderDto>>(controller);

    public IViewEditor<RepositoryRef> View(RepositoryEditorCtrl controller) => Registry.Resolve<RepositoryEditorCtrl, IViewEditor<RepositoryRef>>(controller);

    public IViewBase NotifyView(KNoteManagmentCtrl controller) => Registry.Resolve<KNoteManagmentCtrl, IViewBase>(controller, key: "Notify");

    public IViewBase AboutView(KNoteManagmentCtrl controller) => Registry.Resolve<KNoteManagmentCtrl, IViewBase>(controller, key: "About");

    public IViewEditor<KMessageDto> View(MessageEditorCtrl controller) => Registry.Resolve<MessageEditorCtrl, IViewEditor<KMessageDto>>(controller);

    public IViewEditor<ResourceDto> View(ResourceEditorCtrl controller) => Registry.Resolve<ResourceEditorCtrl, IViewEditor<ResourceDto>>(controller);

    public IViewEditor<KAttributeDto> View(AttributeEditorCtrl controller) => Registry.Resolve<AttributeEditorCtrl, IViewEditor<KAttributeDto>>(controller);

    public IViewEditor<NoteTaskDto> View(TaskEditorCtrl controller) => Registry.Resolve<TaskEditorCtrl, IViewEditor<NoteTaskDto>>(controller);

    public IViewSelector<NoteTypeDto> View(NoteTypesSelectorCtrl controller) => Registry.Resolve<NoteTypesSelectorCtrl, IViewSelector<NoteTypeDto>>(controller);

    public IViewEditor<NoteKAttributeDto> View(NoteAttributeEditorCtrl controller) => Registry.Resolve<NoteAttributeEditorCtrl, IViewEditor<NoteKAttributeDto>>(controller);

    public IViewSelector<SelectedNotesInServiceRef> View(FiltersSelectorCtrl controller) => Registry.Resolve<FiltersSelectorCtrl, IViewSelector<SelectedNotesInServiceRef>>(controller);

    public IViewEditor<AppConfig> View(OptionsEditorCtrl controller) => Registry.Resolve<OptionsEditorCtrl, IViewEditor<AppConfig>>(controller);

    public IViewBase View(KntChatGPTCtrl controller) => Registry.Resolve<KntChatGPTCtrl, IViewBase>(controller);

    public IViewChat View(KntChatCtrl controller) => Registry.Resolve<KntChatCtrl, IViewChat>(controller);

    public IViewServerCOM View(KntServerCOMCtrl controller) => Registry.Resolve<KntServerCOMCtrl, IViewServerCOM>(controller);

    public IViewBase View(KntLabCtrl controller) => Registry.Resolve<KntLabCtrl, IViewBase>(controller);

    public IViewHeavyProcess View(HeavyProcessCtrl controller) => Registry.Resolve<HeavyProcessCtrl, IViewHeavyProcess>(controller);
}
