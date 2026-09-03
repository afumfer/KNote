using KNote.MessageBroker;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository;
using KNote.Service.Core;
using KNote.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// Minimal IKntService test double exposing only a FakeKntNoteService via Notes; every other
/// member throws NotSupportedException, so a test that unexpectedly reaches one fails loudly.
/// </summary>
internal class FakeKntService : IKntService
{
    public FakeKntNoteService NotesFake { get; } = new();
    public FakeKntUserService UsersFake { get; } = new();
    public FakeKntNoteTypeService NoteTypesFake { get; } = new();
    public FakeKntTraceNoteTypeService TraceNoteTypesFake { get; } = new();
    public FakeKntKAttributeService KAttributesFake { get; } = new();

    public ILogger Logger { get; set; }
    public Guid IdServiceRef { get; } = Guid.NewGuid();
    public RepositoryRef RepositoryRef => throw new NotSupportedException();
    public string UserIdentityName { get; set; }

    public IKntRepository Repository => throw new NotSupportedException();
    public IKntUserService Users => UsersFake;
    public IKntKAttributeService KAttributes => KAttributesFake;
    public IKntSystemValuesService SystemValues => throw new NotSupportedException();
    public IKntFolderService Folders => throw new NotSupportedException();
    public IKntNoteService Notes => NotesFake;
    public IKntNoteTypeService NoteTypes => NoteTypesFake;
    public IKntTraceNoteTypeService TraceNoteTypes => TraceNoteTypesFake;
    public IKntMessageBroker MessageBroker => throw new NotSupportedException();

    public Task<bool> TestDbConnection() => throw new NotSupportedException();
    public Task<bool> CreateDataBase(string newOwner = null) => throw new NotSupportedException();
    public string GetSystemVariable(string scope, string variable) => throw new NotSupportedException();
    public void SaveSystemVariable(string scope, string key, string value) => throw new NotSupportedException();
    public void PublishNoteInMessageBroker(NoteExtendedDto noteInfo) => throw new NotSupportedException();
    public string ReplaceSpecialCharacters(string text) => throw new NotSupportedException();

    public void Dispose() { }
}
