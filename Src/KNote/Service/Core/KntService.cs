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

namespace KNote.Service.Core
{
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
                    //_noteTypes = new KntNoteTypeService(_repository);
                    _noteTypes = new KntNoteTypeService(this);
                return _noteTypes;
            }
        }

        public RepositoryRef RepositoryRef
        {
            get { return _repository.RespositoryRef; }
        }

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

        #endregion

        #region IDisposable member

        public void Dispose()
        {
            // TODO: call dispose all properties with reflection ?? 

            if (_repository != null)
                _repository.Dispose();
        }

        #endregion
    }
}
