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
        get
        {
            if (_messageBroker == null)
            {
                // TODO: !!!

                // Get params from database system variables. Validate params !!!

                // Connection
                string hostName = GetSystemVariable("KNT_MESSAGEBROKER_CONNECTION", "HOST_NAME");
                string virtualHost = GetSystemVariable("KNT_MESSAGEBROKER_CONNECTION", "VIRTUAL_HOST");
                int port = int.Parse(GetSystemVariable("KNT_MESSAGEBROKER_CONNECTION", "PORT"));
                string userName = GetSystemVariable("KNT_MESSAGEBROKER_CONNECTION", "USER_NAME"); 
                string password = GetSystemVariable("KNT_MESSAGEBROKER_CONNECTION", "PASSWORD");

                string exchangePublish = GetSystemVariable("KNT_MESSAGEBROKER_CONFIG_PUBLISH", "EXCHANGE_PUBLISH");  // Echange;Type                
                string exchangeConsume = GetSystemVariable("KNT_MESSAGEBROKER_CONFIG_CONSUME", "EXCHANGE_CONSUME_1");  // queue;bind-echange;routing

                var exchangePublishValues = exchangePublish.Split(';');
                var exchangeConsumeValues = exchangeConsume.Split(';');

                _messageBroker = new KntMessageBroker(hostName, virtualHost, port, userName, password);                

                // ExchangeDeclare
                //_messageBroker.ExchangeDeclare(exchange, type);
                //_messageBroker.ExchangeDeclare("ex.FanoutArmando1", "fanout");  // Test                
                _messageBroker.ExchangeDeclare(exchangePublishValues[0], exchangePublishValues[1]);  // Test

                // QueueDeclare
                //_messageBroker.QueueDeclare(queue);
                //_messageBroker.QueueDeclare("cola.Armando1");  // Test
                _messageBroker.QueueDeclare(exchangeConsumeValues[0]);  // Test

                // QueueBind
                //_messageBroker.QueueBind(queue, exchange, routingKey);
                //_messageBroker.QueueBind("cola.Armando1", "ex.FanoutArmando1", "");   // Test
                _messageBroker.QueueBind(exchangeConsumeValues[0], exchangeConsumeValues[1], exchangeConsumeValues[2]);   // Test

                //_messageBroker.ConsumerReceived += _messageBroker_ConsumerReceived;

                _messageBroker.ConsumerReceived += async (sender, e) =>
                {
                    var n = await Notes.NewAsync();
                    var note = n.Entity;
                    note.Topic = e.Entity;

                    var f = await Folders.GetHomeAsync();
                    note.FolderId = f.Entity.FolderId;

                    await Notes.SaveAsync(note);
                };


                _messageBroker.BasicConsume(exchangeConsumeValues[0]);
            }
            return _messageBroker;            
        }
    }

    //private async void _messageBroker_ConsumerReceived(object sender, MessageBusEventArgs<string> e)
    //{
    //    var n = await Notes.NewAsync();
    //    var note = n.Entity;
    //    note.Topic = e.Entity;
        
    //    var f = await Folders.GetHomeAsync();
    //    note.FolderId = f.Entity.FolderId;

    //    await Notes.SaveAsync(note);
    //}

    #endregion

    public string GetSystemVariable(string scope, string variable)
    {
        var valueDto = Task.Run(() => SystemValues.GetAsync(new KeyValuePair<string, string>(scope, variable))).Result;
        if (valueDto.IsValid)
            return valueDto.Entity.Value;
        else
            return "";
    }


    #region IDisposable member

    public void Dispose()
    {
        // TODO: call dispose all properties

        if (_repository != null)
            _repository.Dispose();
    }

    #endregion
}
