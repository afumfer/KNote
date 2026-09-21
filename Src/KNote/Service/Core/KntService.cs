using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using KNote.Repository;
using KNote.Model.Dto;
using KNote.Model;
using KNote.Service.Interfaces;
using KNote.Service.Services;
using KNote.MessageBroker;
using KNote.MessageBroker.RabbitMQ;
using Microsoft.IdentityModel.Tokens;
using System.Xml.XPath;
using System.IO;

using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;
//using NLog;

namespace KNote.Service.Core;

public class KntService : IKntService, IDisposable
{
    #region Constructors

    public KntService(IKntRepository repository, bool activateMessageBroker = false)
    {
        _repository = repository;
        IdServiceRef = Guid.NewGuid();

        // Experimental ------------
        if(!activateMessageBroker)
        {
            _messageBroker.Enabled = false;
            _messageBroker.StatusInfo = $"{KntConst.AppName} message bus in not activated.";
        }
        // Callers that pass activateMessageBroker: true must also await
        // InitMessageBrokerAsync() - a constructor cannot await the
        // RabbitMQ.Client 7.x async connection handshake.
        //--------------------------
    }

    #endregion

    #region IKntService members

    public Guid IdServiceRef { get; }

    private readonly IKntRepository _repository;
    public IKntRepository Repository
    {
        get { return _repository; }
    }

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

    private IKntTraceNoteTypeService _traceNoteTypes;
    public IKntTraceNoteTypeService TraceNoteTypes
    {
        get
        {
            if (_traceNoteTypes == null)
                _traceNoteTypes = new KntTraceNoteTypeService(this);
            return _traceNoteTypes;
        }
    }

    public RepositoryRef RepositoryRef
    {
        get { return _repository.RepositoryRef; }
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
                return false;

            if (!string.IsNullOrEmpty(newOwner))
            {
                var resGetU = await Users.GetByUserNameAsync("owner");
                if (resGetU.IsValid)
                {
                    resGetU.Entity.UserName = newOwner;
                    var resUpdateU = await Users.SaveAsync(resGetU.Entity);
                    if (!resUpdateU.IsValid)
                        return false;
                }
                else
                    return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }


    private IKntMessageBroker _messageBroker = new KntMessageBroker();
    public IKntMessageBroker MessageBroker
    {
        get { return _messageBroker; }
    }


    public ILogger Logger 
    { 
        get ; set; 
    }

    public string GetSystemVariable(string scope, string key)
    {
        var valueDto = Task.Run(() => SystemValues.GetAsync(new KeyValuePair<string, string>(scope, key))).Result;
        if (valueDto.IsValid)
            return valueDto.Entity.Value;
        else
            return "";
    }

    public List<string> GetSystemVariables(string scope)
    {
        var result = new List<string>();
        var values = Task.Run(() => SystemValues.GetAllAsync()).Result;
        if (values.IsValid)
        {
            result = values.Entity.Where(sv => sv.Scope == scope).Select(sv => sv.Value).ToList();
            return result;
        }
        else
            return result;
    }

    // Concurrency notes: this used to decide insert-vs-update from a stale read (two concurrent
    // callers could both see "no row yet" and both try to insert, racing on the unique index over
    // Scope+Key) and discarded the save's result/exceptions via an unawaited Task. Fixed to retry
    // as an update when a concurrent caller creates the row first - detected via
    // KntUniqueConstraintViolationException, a provider-agnostic marker Repository.Dapper/
    // Repository.EntityFramework throw specifically for this case (see
    // KntSystemValuesRepository.AddAsync in each), rather than matching on exception message text.
    // Two callers reading the same existing row and both updating it (a lost update) is not
    // detected here, since UpdateAsync has no optimistic concurrency check - fixing that would need
    // a concurrency token on SystemValues, out of proportion for what is otherwise a plain
    // key/value setting.
    public void SaveSystemVariable(string scope, string key, string value)
    {
        const int maxAttempts = 5;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var valueDto = Task.Run(() => SystemValues.GetAsync(new KeyValuePair<string, string>(scope, key))).Result;

            var saveDto = valueDto.IsValid
                ? new SystemValueDto { SystemValueId = valueDto.Entity.SystemValueId, Scope = scope, Key = key, Value = value }
                : new SystemValueDto { SystemValueId = Guid.Empty, Scope = scope, Key = key, Value = value };

            try
            {
                var saveResult = Task.Run(() => SystemValues.SaveAsync(saveDto)).Result;

                if (!saveResult.IsValid)
                    throw new KntServiceException($"Could not save system variable '{scope}/{key}': {saveResult.ErrorMessage}");

                return;
            }
            catch (Exception ex) when (attempt < maxAttempts && ContainsUniqueConstraintViolation(ex))
            {
                // A concurrent caller inserted this Scope+Key row first; retry, this time reading
                // and updating it instead of inserting.
            }
        }

        throw new KntServiceException($"Could not save system variable '{scope}/{key}' after {maxAttempts} attempts due to concurrent writes.");
    }

    private static bool ContainsUniqueConstraintViolation(Exception ex)
    {
        for (var current = ex; current != null; current = current.InnerException)
        {
            if (current is KntUniqueConstraintViolationException)
                return true;
        }

        return false;
    }

    public void PublishNoteInMessageBroker(NoteExtendedDto noteInfo)
    {
        if (_messageBroker.Enabled)
        {
            var noteSerialized = JsonSerializer.Serialize(noteInfo);
            Task.Run(() => _messageBroker.BasicPublishAsync(noteSerialized, "")).GetAwaiter().GetResult();
        }
    }

    public string ReplaceSpecialCharacters(string text)
    {
        // Normalize and remove accents
        string normalized = text.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder();

        foreach (char c in normalized)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark) // Exclude accent marks
            {
                sb.Append(c);
            }
        }

        string withoutAccents = sb.ToString().Normalize(NormalizationForm.FormC);

        // Convert to ASCII 127 by replacing spaces and special characters
        string result = Regex.Replace(withoutAccents, @"[^a-zA-Z0-9.]", "_");

        return result;
    }

    #endregion

    #region Command execution events

    public event EventHandler<CommandExecutingEventArgs> CommandExecuting;
    public event EventHandler<CommandExecutedEventArgs> CommandExecuted;

    public void NotifyCommandExecuting(CommandExecutingEventArgs e) => RaiseSafely(CommandExecuting, e);

    public void NotifyCommandExecuted(CommandExecutedEventArgs e) => RaiseSafely(CommandExecuted, e);

    // Invokes each subscriber individually so one throwing handler (e.g. a buggy audit/telemetry
    // listener) can never abort the command whose execution is being reported.
    private void RaiseSafely<TArgs>(EventHandler<TArgs> handler, TArgs e) where TArgs : EventArgs
    {
        if (handler == null)
            return;

        foreach (var single in handler.GetInvocationList())
        {
            try
            {
                ((EventHandler<TArgs>)single).Invoke(this, e);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Command execution event subscriber threw for {eventArgs}", typeof(TArgs).Name);
            }
        }
    }

    #endregion

    #region Message broker, experimental ....

    // Public and async because the RabbitMQ.Client 7.x handshake is Task-based and a
    // constructor cannot await it - callers passing activateMessageBroker: true to the
    // constructor must call this explicitly afterwards (see KntExtensions.KntConfigureMessageBroker).
    public async Task InitMessageBrokerAsync()
    {
        try
        {
            // Connection
            string enabledValue = GetSystemVariable("KNT_MESSAGEBROKER_CONNECTION", "ENABLED");  // True or False
            bool enabled;
            bool.TryParse(enabledValue, out enabled);
            if (string.IsNullOrEmpty(enabledValue) || !enabled )
            {
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
                _messageBroker.Enabled = false;
                _messageBroker.StatusInfo = $"{KntConst.AppName} message bus not initialized. Connection parameters not set.";
                return;
            }

            // Publisher and consumers config            
            string publisher = GetSystemVariable("KNT_MESSAGEBROKER_CONFIG_PUBLISH", "EXCHANGE_PUBLISH");  // Echange;Type        
            var queuesConsume = GetSystemVariables("KNT_MESSAGEBROKER_CONFIG_CONSUME");  // queue;bind-echange;routing            

            // KntMessageBroker configuration
            await _messageBroker.CreateConnectionAsync(hostName, virtualHost, port, userName, password);

            if(!string.IsNullOrEmpty(publisher))
                await _messageBroker.PublishDeclareAsync(publisher);

            //if (!string.IsNullOrEmpty(queueConsume))
            if (queuesConsume.Count > 0)
            {
                await _messageBroker.QueuesBindAsync(queuesConsume);

                _messageBroker.ConsumerReceived += (sender, e) =>
                {
                    // Important, this method must be synchronous
                    OnSaveNoteEventBus(e.Entity);
                };

                foreach (var queue in _messageBroker.QueuesConsume)
                    await _messageBroker.BasicConsumeAsync(queue);
            }
                       
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
            // SaveSystemVariable("KNT_MESSAGEBROKER_CONNECTION", "ENABLED", "False");  /// NOT for now ...
        }
    }

    private void OnSaveNoteEventBus(string noteStr)
    {    
        var noteInput = JsonSerializer.Deserialize<NoteExtendedDto>(noteStr);

        // Reset and override values for no importable attributes for this repository.                        
        
        var resExisting = (Task.Run(() => Notes.GetAsync(noteInput.NoteId)).Result);

        if (resExisting.Entity == null)
            noteInput.NoteNumber = 0;
        else
            noteInput.NoteNumber = resExisting.Entity.NoteNumber;        
        var f = (Task.Run(() => Folders.GetHomeAsync()).Result);
        noteInput.FolderId = f.Entity.FolderId;
        noteInput.FolderDto = f.Entity;
        noteInput.KAttributesDto = null;
        noteInput.NoteTypeId = null;
        noteInput.NoteTypeDto = null;
        noteInput.Tags = noteInput.Tags.Replace(KntConst.TagForMerging, "");
        foreach (var r in noteInput.Resources)
        {            
            noteInput.Description = noteInput.Description.Replace(r.Container.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), 
                Repository.RepositoryRef.ResourcesContainer);            
            r.Container = Repository.RepositoryRef.ResourcesContainer;
            r.ContentInDB = Repository.RepositoryRef.ResourceContentInDB;
        }
        
        Task.Run(() => Notes.SaveExtendedAsync(noteInput)).Wait();
    }

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
