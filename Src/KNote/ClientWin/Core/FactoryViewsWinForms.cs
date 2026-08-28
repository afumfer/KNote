using KNote.ClientWin.Controllers;
using KNote.ClientWin.Views;
using KNote.Model;
using KNote.Model.Dto;

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
        Registry.Register<UserRegisterCtrl, IViewEditor<UserRegisterDto>>(c => new UserRegisterForm(c));
        Registry.Register<UserEditorCtrl, IViewEditor<UserDto>>(c => new UserEditorForm(c));
        Registry.Register<UsersManageCtrl, IViewManageList<UserDto>>(c => new UsersManageForm(c));
        Registry.Register<KNoteManagmentCtrl, IViewBase>(c => new NotifyForm(c), key: "Notify");
        Registry.Register<KNoteManagmentCtrl, IViewBase>(c => new KNoteAboutForm(c), key: "About");
        Registry.Register<MessageEditorCtrl, IViewEditor<KMessageDto>>(c => new MessageEditorForm(c));
        Registry.Register<ResourceEditorCtrl, IViewEditor<ResourceDto>>(c => new ResourceEditorForm(c));
        Registry.Register<AttributeEditorCtrl, IViewEditor<KAttributeDto>>(c => new AttributeEditorForm(c));
        Registry.Register<KAttributeTabulatedValueEditorCtrl, IViewEditor<KAttributeTabulatedValueDto>>(c => new KAttributeTabulatedValueEditorForm(c));
        Registry.Register<KAttributesManageCtrl, IViewManageList<KAttributeInfoDto>>(c => new KAttributesManageForm(c));
        Registry.Register<TaskEditorCtrl, IViewEditor<NoteTaskDto>>(c => new TaskEditorForm(c));
        Registry.Register<NoteTypesSelectorCtrl, IViewSelector<NoteTypeDto>>(c => new NoteTypesSelectorForm(c));
        Registry.Register<NoteTypeEditorCtrl, IViewEditor<NoteTypeDto>>(c => new NoteTypeEditorForm(c));
        Registry.Register<NoteTypesManageCtrl, IViewManageList<NoteTypeDto>>(c => new NoteTypesManageForm(c));
        Registry.Register<NoteAttributeEditorCtrl, IViewEditor<NoteKAttributeDto>>(c => new NoteAttributeEditorForm(c));
        Registry.Register<FiltersSelectorCtrl, IViewSelector<SelectedNotesInServiceRef>>(c => new FiltersSelectorForm(c));
        Registry.Register<OptionsEditorCtrl, IViewEditor<AppConfig>>(c => new OptionsEditorForm(c));
        Registry.Register<KntChatGPTCtrl, IViewBase>(c => new KntChatGPTForm(c));
        Registry.Register<KNoteAIAssistantCtrl, IViewBase>(c => new KNoteAIAssistantForm(c));
        Registry.Register<AiProviderEditorCtrl, IViewEditor<AiProviderRef>>(c => new AiProviderEditorForm(c));
        Registry.Register<AiProvidersManageCtrl, IViewManageList<AiProviderRef>>(c => new AiProvidersManageForm(c));
        Registry.Register<KntChatCtrl, IViewChat>(c => new KntChatForm(c));
        Registry.Register<KntServerCOMCtrl, IViewServerCOM>(c => new KntServerCOMForm(c));
        Registry.Register<KntLabCtrl, IViewBase>(c => new KntLabForm(c));
        Registry.Register<HeavyProcessCtrl, IViewHeavyProcess>(c => new HeavyProcessForm(c));
    }
}
