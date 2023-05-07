using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.ComponentModel;
using System.Linq.Expressions;
using KNote.Repository;
using KNote.Repository.EntityFramework;
using DP = KNote.Repository.Dapper;
using KNote.Model.Dto;
using KNote.Model;
using KNote.Service.Interfaces;
using KNote.Service.Services;
using System.Threading;
using KNote.MessageBroker;
using KNote.MessageBroker.RabbitMQ;
using System.Collections;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Threading.Channels;
using Microsoft.Identity.Client;
using static System.Formats.Asn1.AsnWriter;
using System.Text.Json;

namespace KNote.Service.Core;

public class KntService : IKntService, IDisposable
{
    #region Properties

    public Guid IdServiceRef { get; }

    private readonly IKntRepository _repository;
    public IKntRepository Repository
    {
        get { return _repository; }
    }

    #endregion 

    #region Constructors

    public KntService(IKntRepository repository)
    {
        _repository = repository;
        IdServiceRef = Guid.NewGuid();
        
        // Experimental ...
        InitMessageBroker();
        // .................
    }

    #endregion

    #region IKntService members

    private IKntUserService _users;
    public IKntUserService Users
    {
        get
        {
            if (_users == null)
                _users = new KntUserService(this);
            return _users;
        }
    }

    private IKntKAttributeService _kattributes;
    public IKntKAttributeService KAttributes
    {
        get
        {
            if (_kattributes == null)
                _kattributes = new KntKAttributeService(this);
            return _kattributes;
        }
    }

    private IKntSystemValuesService _systemValues;
    public IKntSystemValuesService SystemValues
    {
        get
        {
            if (_systemValues == null)
                _systemValues = new KntSystemValuesService(this);
            return _systemValues;
        }
    }

    private IKntFolderService _folders;
    public IKntFolderService Folders
    {
        get
        {
            if (_folders == null)
                _folders = new KntFolderService(this);
            return _folders;
        }
    }

    private IKntNoteService _notes;
    public IKntNoteService Notes
    {
        get
        {
            if (_notes == null)
                _notes = new KntNoteService(this);
            return _notes;
        }
    }

    private IKntNoteTypeService _noteTypes;
    public IKntNoteTypeService NoteTypes
    {
        get
        {
            if (_noteTypes == null)                    
                _noteTypes = new KntNoteTypeService(this);
            return _noteTypes;
        }
    }

    public RepositoryRef RepositoryRef
    {
        get { return _repository.RespositoryRef; }
    }

    public string UserIdentityName { get; set; }
    
    public async Task<bool> TestDbConnection()
    {
        return await _repository.TestDbConnection();
    }

    public async Task<bool> CreateDataBase(string newOwner = null)
    {
        try
        {
            var res = await SystemValues.GetAllAsync();
            if (!res.IsValid)
                return await Task.FromResult(false);

            if (!string.IsNullOrEmpty(newOwner))
            {
                var resGetU = await Users.GetByUserNameAsync("owner");
                if (resGetU.IsValid)
                {
                    resGetU.Entity.UserName = newOwner;
                    var resUpdateU = await Users.SaveAsync(resGetU.Entity);
                    if (!resUpdateU.IsValid)
                        return await Task.FromResult(false);
                }
                else
                    return await Task.FromResult(false);
            }
        }
        catch (Exception)
        {
            return await Task.FromResult(false);
        }
        return await Task.FromResult(true);
    }


    private IKntMessageBroker _messageBroker;
    public IKntMessageBroker MessageBroker
    {
        get { return _messageBroker; }
    }

    public string GetSystemVariable(string scope, string key)
    {
        var valueDto = Task.Run(() => SystemValues.GetAsync(new KeyValuePair<string, string>(scope, key))).Result;
        if (valueDto.IsValid)
            return valueDto.Entity.Value;
        else
            return "";
    }

    public void SaveSystemVariable(string scope, string key, string value)
    {
        Guid id = Guid.Empty;
        var valueDto = Task.Run(() => SystemValues.GetAsync(new KeyValuePair<string, string>(scope, key))).Result;
        if (valueDto.IsValid)
            id = valueDto.Entity.SystemValueId;
        var res = Task.Run(() => SystemValues.SaveAsync(new SystemValueDto { SystemValueId = id, Scope = scope, Key = key, Value = value }));
    }

    public void PublishNoteInMessageBroker(NoteExtendedDto noteInfo)
    {
        if (_messageBroker.Enabled)
        {
            var noteSerialized = JsonSerializer.Serialize(noteInfo);
            _messageBroker.BasicPublish(noteSerialized, "");
        }
    }

    // TODO: In the future, implement an alternative to get the notes counter. This is a possibility.
    public int GetNextNoteNumber()
    {
        var scope = "SYSTEM";
        var key = "NOTES_COUNTER";
        Guid id = Guid.Empty;
        var resNextNoteNumber = 0;

        var valueDto = Task.Run(() => SystemValues.GetAsync(new KeyValuePair<string, string>(scope, key))).Result;
        if (valueDto.IsValid)
        {
            resNextNoteNumber = int.Parse(valueDto.Entity.Value) + 1;
        }
        {
            resNextNoteNumber = 4000; // !!! TODO: replace for count notes 
        }
        var res = Task.Run(() => SystemValues.SaveAsync(new SystemValueDto { SystemValueId = id, Scope = scope, Key = key, Value = resNextNoteNumber.ToString() }));
        return resNextNoteNumber;

    }

    #endregion

    #region Private methods

    #region Message broker, experimental .... 

    private void InitMessageBroker()
    {
        try
        {
            // Connection
            string enabledValue = GetSystemVariable("KNT_MESSAGEBROKER_CONNECTION", "ENABLED");  // True or False
            bool enabled;
            bool.TryParse(enabledValue, out enabled);
            if (string.IsNullOrEmpty(enabledValue) || !enabled )
            {
                if (_messageBroker == null)
                    _messageBroker = new KntMessageBroker();
                _messageBroker.Enabled = false;
                _messageBroker.StatusInfo = $"{KntConst.AppName} message bus not enabled.";
                return;
            }
            
            string hostName = GetSystemVariable("KNT_MESSAGEBROKER_CONNECTION", "HOST_NAME");
            string virtualHost = GetSystemVariable("KNT_MESSAGEBROKER_CONNECTION", "VIRTUAL_HOST");
            int port = int.Parse(GetSystemVariable("KNT_MESSAGEBROKER_CONNECTION", "PORT"));
            string userName = GetSystemVariable("KNT_MESSAGEBROKER_CONNECTION", "USER_NAME");
            string password = GetSystemVariable("KNT_MESSAGEBROKER_CONNECTION", "PASSWORD");
            if (string.IsNullOrEmpty(hostName) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                if (_messageBroker == null)
                    _messageBroker = new KntMessageBroker();
                _messageBroker.Enabled = false;
                _messageBroker.StatusInfo = $"{KntConst.AppName} message bus not initialized. Connection parameters not set.";
                return;
            }

            // Publisher and consumers config
            var queuesConsume = new List<string>();
            string publisher = GetSystemVariable("KNT_MESSAGEBROKER_CONFIG_PUBLISH", "EXCHANGE_PUBLISH");  // Echange;Type        
            string queueConsume = GetSystemVariable("KNT_MESSAGEBROKER_CONFIG_CONSUME", "EXCHANGE_CONSUME_1");  // queue;bind-echange;routing
            //TODO: more queues here ... (KNT_MESSAGEBROKER_CONFIG_CONSUME is a collection).
            queuesConsume.Add(queueConsume);

            // KntMessageBroker configuration
            _messageBroker = new KntMessageBroker(hostName, virtualHost, port, userName, password);
            _messageBroker.PublishDeclare(publisher);
            _messageBroker.QueuesBind(queuesConsume);            
            
            _messageBroker.ConsumerReceived += (sender, e) =>
            {
                // Important, this method must be synchronous
                OnSaveNoteEventBus(e.Entity);                
            };
            
            foreach (var queue in _messageBroker.QueuesConsume)
                _messageBroker.BasicConsume(queue);
            
            _messageBroker.Enabled = bool.Parse(enabledValue);
            if(_messageBroker.Enabled)
                _messageBroker.StatusInfo = $"{KntConst.AppName} message bus initialized.";
            else
                _messageBroker.StatusInfo = $"{KntConst.AppName} message bus initialized, but not enabled.";
        }
        catch (Exception ex)
        {
            if(_messageBroker == null)
                _messageBroker = new KntMessageBroker();
            _messageBroker.Enabled = false;
            _messageBroker.StatusInfo = ex.Message.ToString();
            SaveSystemVariable("KNT_MESSAGEBROKER_CONNECTION", "ENABLED", "False");
        }
    }

    private void OnSaveNoteEventBus(string noteStr)
    {    
        var noteInput = JsonSerializer.Deserialize<NoteExtendedDto>(noteStr);

        // Reset and override values for no importable attributes for this repository.                        
        
        var resExisting = (Task.Run(() => Notes.GetAsync(noteInput.NoteId)).Result);

        if (resExisting.Entity == null)
            noteInput.NoteNumber = 0;  //GetNextNoteNumber();
        else
            noteInput.NoteNumber = resExisting.Entity.NoteNumber;
        noteInput.Topic += $" - (Merged: {DateTime.Now})";
        var f = (Task.Run(() => Folders.GetHomeAsync()).Result);
        noteInput.FolderId = f.Entity.FolderId;
        noteInput.FolderDto = f.Entity;
        noteInput.KAttributesDto = null;
        noteInput.NoteTypeId = null;
        noteInput.NoteTypeDto = null;
        noteInput.Tags = noteInput.Tags.Replace(KntConst.TagForMerging, "");
        foreach (var r in noteInput.Resources)
        {
            // TODO: refactor this conversión pending ...
            noteInput.Description = noteInput.Description.Replace(r.Container.Replace(@"\", @"/"), Repository.RespositoryRef.ResourcesContainer);
            r.Container = Repository.RespositoryRef.ResourcesContainer;
            r.ContentInDB = Repository.RespositoryRef.ResourceContentInDB;
        }
        
        Task.Run(() => Notes.SaveExtendedAsync(noteInput)).Wait();
    }

    #endregion 

    #endregion

    #region IDisposable member

    public void Dispose()
    {
        // TODO: call dispose all properties

        if (_repository != null)
            _repository.Dispose();
    }

    #endregion
}
