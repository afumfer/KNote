using System;
using System.Threading.Tasks;
using KNote.MessageBroker;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository;
using KNote.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace KNote.Service.Core;

public interface IKntService : IDisposable
{
    ILogger Logger { get; set; }
    Guid IdServiceRef { get; }
    RepositoryRef RepositoryRef { get; }
    Task<bool> TestDbConnection();
    Task<bool> CreateDataBase(string newOwner = null);
    string UserIdentityName { get; set; }

    IKntRepository Repository { get; }

    IKntUserService Users { get; }
    IKntKAttributeService KAttributes { get; }
    IKntSystemValuesService SystemValues { get; }
    IKntFolderService Folders { get; }
    IKntNoteService Notes { get; }
    IKntNoteTypeService NoteTypes { get; }
    IKntMessageBroker MessageBroker { get; }

    string GetSystemVariable(string scope, string variable);
    void SaveSystemVariable(string scope, string key, string value);
    void PublishNoteInMessageBroker(NoteExtendedDto noteInfo);
    string ReplaceSpecialCharacters(string text);
}
