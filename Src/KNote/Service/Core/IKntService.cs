using System;
using System.Threading.Tasks;
using KNote.MessageBroker;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository;
using KNote.Service.Interfaces;

namespace KNote.Service.Core;

public interface IKntService : IDisposable
{
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
}
