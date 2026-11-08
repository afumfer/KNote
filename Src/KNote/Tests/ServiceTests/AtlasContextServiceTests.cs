using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The context KNote Atlas gives the AI through the Service layer (IKntAtlasContextService, stage 1, delivery 1.1): the
/// windows of the profile, the glossary and the task instructions, the creation of their notes (once, in «KNote Atlas»),
/// «Fill in folders and types», what the AI receives, and the rule that only ProjectManager and Admin write those notes
/// (also with the authorization of commands off, as in the Server).
/// </summary>
[TestClass]
public class AtlasContextServiceTests
{
    private const string NotAllowed = "Only a ProjectManager or an Admin";

    private static async Task<KntService> ServiceAsync(RepositoryTestDatabase db, string orm, string userName, string role, bool enforce = true)
    {
        var service = new KntService(db.CreateRepository(orm)) { UserIdentityName = userName, EnforceAuthorization = enforce };
        if ((await service.Repository.Users.GetByUserNameAsync(userName)).Entity == null)
        {
            var res = await service.Repository.Users.AddAsync(new UserDto
            {
                UserId = Guid.NewGuid(),
                UserName = userName,
                EMail = $"{userName}-{Guid.NewGuid():N}@knote.tests",
                FullName = $"{userName} full name",
                RoleDefinition = role
            });
            Assert.IsTrue(res.IsValid, res.ErrorMessage);
        }
        return service;
    }

    private static Task<KntService> AdminAsync(RepositoryTestDatabase db, string orm = "Dapper") => ServiceAsync(db, orm, "atlas-admin", "Admin");

    private static async Task<NoteDto> SaveDescriptionAsync(KntService service, Guid noteId, string description)
    {
        var note = (await service.Notes.GetAsync(noteId)).Entity;
        note.Description = description;
        var res = await service.Notes.SaveAsync(note);
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity;
    }

    private static bool HasBareLf(string text) => text.Replace("\r\n", "").Contains('\n');

    private static async Task SetAttributeAsync(KntService service, Guid noteId, string name, string value)
    {
        var note = (await service.Notes.GetAsync(noteId)).Entity;
        note.KAttributesDto.Single(a => a.Name == name).Value = value;
        Assert.IsTrue((await service.Notes.SaveAsync(note)).IsValid);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheWindows_WithoutTheNotes_SayItAndCreateNothing(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var guest = await ServiceAsync(db, orm, "atlas-guest", "Guest");

        var profile = await guest.AtlasContext.GetProfileInfoAsync();
        var glossary = await guest.AtlasContext.GetGlossaryInfoAsync();
        var tasks = await guest.AtlasContext.GetTaskPromptsAsync();

        Assert.IsTrue(profile.IsValid, profile.ErrorMessage);
        Assert.IsNull(profile.Entity.Note);
        Assert.IsNull(glossary.Entity.Note);
        Assert.AreEqual(EnumAtlasTaskPromptState.Default, tasks.Entity.Single().State);
        var types = (await guest.NoteTypes.GetAllAsync()).Entity;
        Assert.IsFalse(types.Any(t => t.Name == KntConst.ProfileTag || t.Name == KntConst.TaskPromptTag));
        Assert.IsFalse((await guest.AtlasContext.CreateProfileAsync()).IsValid, "a Guest does not create it");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheProfileAndTheGlossary_AreCreatedOnce_WithTheirTemplates(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var admin = await AdminAsync(db, orm);

        var first = await admin.AtlasContext.CreateProfileAsync();
        var second = await admin.AtlasContext.CreateProfileAsync();
        var glossary = await admin.AtlasContext.CreateGlossaryAsync();

        Assert.IsTrue(first.IsValid, first.ErrorMessage);
        Assert.AreEqual(first.Entity.NoteId, second.Entity.NoteId);
        Assert.AreEqual(KntConst.ProfileTopic, first.Entity.Topic);
        Assert.AreEqual(AtlasTemplates.NoteText(AtlasTemplates.Profile), first.Entity.Description);
        Assert.AreEqual(AtlasTemplates.NoteText(GlossaryTable.Template), glossary.Entity.Description);
        Assert.IsFalse(HasBareLf(first.Entity.Description) || HasBareLf(glossary.Entity.Description), "CR+LF");
        var folders = (await admin.Folders.GetAllAsync()).Entity;
        Assert.AreEqual(1, folders.Count(f => f.Name == KntConst.AtlasFolderName));
        Assert.AreEqual(first.Entity.FolderId, glossary.Entity.FolderId);

        var info = (await admin.AtlasContext.GetProfileInfoAsync()).Entity;
        Assert.AreEqual(first.Entity.NoteNumber, info.Note.NoteNumber);
        Assert.AreEqual(0, info.Words, "the help of the template is not counted");
        var glossaryInfo = (await admin.AtlasContext.GetGlossaryInfoAsync()).Entity;
        Assert.AreEqual(0, glossaryInfo.Terms);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Customize_CreatesTheNoteOfTheTask_InModeAdd_AndItsStateFollowsItsAttributes(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var admin = await AdminAsync(db, orm);

        var res = await admin.AtlasContext.CustomizeTaskAsync("Assistant");

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        var note = (await admin.Notes.GetAsync(res.Entity.NoteId)).Entity;
        Assert.AreEqual("Assistant", note.KAttributesDto.Single(a => a.Name == KntConst.TaskPromptTaskAttributeName).Value);
        Assert.AreEqual("Add", note.KAttributesDto.Single(a => a.Name == KntConst.TaskPromptModeAttributeName).Value);
        Assert.AreEqual(EnumKAttributeDataType.TabulatedValue, note.KAttributesDto.Single(a => a.Name == KntConst.TaskPromptModeAttributeName).KAttributeDataType);
        Assert.AreEqual(EnumKAttributeDataType.Bool, note.KAttributesDto.Single(a => a.Name == KntConst.TaskPromptActiveAttributeName).KAttributeDataType);
        Assert.IsFalse(HasBareLf(note.Description), "CR+LF");
        Assert.AreEqual(res.Entity.NoteId, (await admin.AtlasContext.CustomizeTaskAsync("Assistant")).Entity.NoteId, "the same note");
        Assert.AreEqual(EnumAtlasTaskPromptState.Added, (await admin.AtlasContext.GetTaskPromptsAsync()).Entity.Single().State);

        await SaveDescriptionAsync(admin, note.NoteId, "Only short answers.");
        await SetAttributeAsync(admin, note.NoteId, KntConst.TaskPromptModeAttributeName, "Replace");
        var replaced = (await admin.AtlasContext.ComposeAsync(new AiComposeRequestDto())).Entity;
        Assert.AreEqual(EnumAtlasTaskPromptState.Replaced, (await admin.AtlasContext.GetTaskPromptsAsync()).Entity.Single().State);
        Assert.AreEqual("Only short answers.", replaced.Layers.Single(l => l.Name == AiPromptComposer.InstructionsLayer).Text);

        await SetAttributeAsync(admin, note.NoteId, KntConst.TaskPromptActiveAttributeName, false.ToString());
        var inactive = (await admin.AtlasContext.GetTaskPromptsAsync()).Entity.Single();
        Assert.AreEqual(EnumAtlasTaskPromptState.Default, inactive.State);
        Assert.AreEqual(note.NoteId, inactive.Note.NoteId, "«Customize» opens it");
        var composed = (await admin.AtlasContext.ComposeAsync(new AiComposeRequestDto())).Entity;
        Assert.AreEqual(AiTaskCatalog.Assistant.DefaultInstructions, composed.Layers.Single(l => l.Name == AiPromptComposer.InstructionsLayer).Text);
        Assert.IsFalse((await admin.AtlasContext.CustomizeTaskAsync("Summary")).IsValid, "not a task");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Compose_HasTheUser_TheProfile_AndTheGlossaryTermsOfTheConversation(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var admin = await AdminAsync(db, orm);
        var profile = (await admin.AtlasContext.CreateProfileAsync()).Entity;
        await SaveDescriptionAsync(admin, profile.NoteId, AtlasTemplates.Profile.Replace("## Purpose\n\n", "## Purpose\n\nA collection of retro computers.\n\n"));
        var glossary = (await admin.AtlasContext.CreateGlossaryAsync()).Entity;
        await SaveDescriptionAsync(admin, glossary.NoteId, GlossaryTable.Template + "| CPC | Amstrad Colour Personal Computer | CPC464 |\n| QL | Sinclair QL | |\n");
        var staff = await ServiceAsync(db, orm, "atlas-staff", "Staff");

        var res = await staff.AtlasContext.ComposeAsync(new AiComposeRequestDto { Texts = { "Where is my cpc464?", "In the box.", "And the QL?" } });

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        var composed = res.Entity;
        StringAssert.Contains(composed.Text, "The user is atlas-staff full name, with the role Staff.");
        StringAssert.Contains(composed.Text, "# Repository profile\n\n## Purpose\n\nA collection of retro computers.");
        CollectionAssert.AreEqual(new[] { "CPC", "QL" }, composed.GlossaryTerms.Select(t => t.Term).ToList());
        Assert.AreEqual($"#{glossary.NoteNumber}", composed.Layers.Single(l => l.Name == AiPromptComposer.GlossaryLayer).Source);
        Assert.AreEqual(2, (await admin.AtlasContext.GetGlossaryInfoAsync()).Entity.Terms);
        var guest = await ServiceAsync(db, orm, "atlas-guest", "Guest");
        Assert.IsTrue((await guest.AtlasContext.ComposeAsync(new AiComposeRequestDto())).NotAuthorized, "Staff, like the assistant");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task FillFoldersAndTypes_AddsWhatIsMissing_WithoutTheSystemOnes_AndOnlyOnce(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var admin = await AdminAsync(db, orm);
        var home = (await admin.Folders.GetAllAsync()).Entity.First(f => f.ParentId == null);
        Assert.IsTrue((await admin.Folders.SaveAsync(new FolderDto { Name = "Projects", ParentId = home.FolderId })).IsValid);
        var type = (await admin.NoteTypes.SaveAsync(new NoteTypeDto { Name = "Computer" })).Entity;
        var unused = (await admin.NoteTypes.SaveAsync(new NoteTypeDto { Name = "Unused type" })).Entity;
        var computer = (await admin.Notes.NewAsync(new NoteInfoDto { NoteTypeId = type.NoteTypeId })).Entity;
        computer.Topic = "Amstrad CPC 464";
        computer.FolderId = home.FolderId;
        computer.NoteTypeId = type.NoteTypeId;
        Assert.IsTrue((await admin.Notes.SaveAsync(computer)).IsValid);

        var res = await admin.AtlasContext.FillProfileFoldersAndTypesAsync();

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        var text = res.Entity.Description;
        StringAssert.Contains(text, $"- {home.Name}:\r\n- {home.Name} › Projects:");
        Assert.IsFalse(HasBareLf(text), "the note keeps its CR+LF");
        StringAssert.Contains(text, "- Computer:");
        Assert.IsFalse(text.Contains(KntConst.AtlasFolderName + ":"));
        Assert.IsFalse(text.Contains(unused.Name));
        Assert.IsFalse(text.Contains("@KNote"));
        Assert.IsTrue(text.IndexOf("- Computer:") > text.IndexOf("## Note types"));

        var again = await admin.AtlasContext.FillProfileFoldersAndTypesAsync();
        Assert.AreEqual(text, again.Entity.Description, "nothing twice");
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task OnlyAProjectManagerOrAnAdmin_WritesTheNotesOfTheContext(bool enforceAuthorization)
    {
        using var db = new RepositoryTestDatabase();
        var admin = await AdminAsync(db);
        var profile = (await admin.AtlasContext.CreateProfileAsync()).Entity;
        var staff = await ServiceAsync(db, "Dapper", "atlas-staff", "Staff", enforceAuthorization);
        var manager = await ServiceAsync(db, "Dapper", "atlas-manager", "ProjectManager", enforceAuthorization);

        var note = (await staff.Notes.GetAsync(profile.NoteId)).Entity;
        note.Description = "Changed by a Staff user.";
        var save = await staff.Notes.SaveAsync(note);
        StringAssert.Contains(save.ErrorMessage, NotAllowed);

        note.NoteTypeId = null;
        StringAssert.Contains((await staff.Notes.SaveAsync(note)).ErrorMessage, NotAllowed, "nor taking its type away");
        StringAssert.Contains((await staff.Notes.DeleteExtendedAsync(profile.NoteId)).ErrorMessage, NotAllowed);
        StringAssert.Contains((await staff.Notes.UtilPatchFolderAsync(profile.NoteId, profile.FolderId)).ErrorMessage, NotAllowed);
        StringAssert.Contains((await staff.Notes.UtilPatchChangeTagsAsync(profile.NoteId, "", "x")).ErrorMessage, NotAllowed);
        Assert.AreEqual(AtlasTemplates.NoteText(AtlasTemplates.Profile), (await admin.Notes.GetAsync(profile.NoteId)).Entity.Description);

        // A note of another type, as always; a ProjectManager and the system (no user), the profile too.
        var other = (await staff.Notes.NewAsync()).Entity;
        other.Topic = "An everyday note";
        other.FolderId = profile.FolderId;
        Assert.IsTrue((await staff.Notes.SaveAsync(other)).IsValid);
        Assert.IsTrue((await manager.Notes.SaveAsync((await manager.Notes.GetAsync(profile.NoteId)).Entity)).IsValid);
        var system = new KntService(db.CreateRepository("Dapper")) { EnforceAuthorization = false };
        Assert.IsTrue((await system.Notes.SaveAsync((await system.Notes.GetAsync(profile.NoteId)).Entity)).IsValid);
    }

    [TestMethod]
    public async Task TheNotesOfTheContext_AreSystemNotes_OutOfTheSemanticIndex()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        await fixture.RunAsync();
        var profile = (await fixture.Service.AtlasContext.CreateProfileAsync()).Entity;
        await SaveDescriptionAsync(fixture.Service, profile.NoteId, "## Purpose\n\nA collection of retro computers.");

        await fixture.RunAsync();

        Assert.AreEqual(0, (await fixture.ChunksAsync(profile.NoteId)).Count);
    }
}
