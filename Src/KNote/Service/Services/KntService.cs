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

namespace KNote.Service.Services
{
    public class KntService: IKntService, IDisposable
    {
        #region Fields

        protected IKntRepository _repository;

        #endregion

        #region Properties

        public Guid IdServiceRef { get; }
        
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
                    _users = new KntUserService(_repository);
                return _users;
            }
        }

        private IKntKAttributeService _kattributes;
        public IKntKAttributeService KAttributes
        {
            get
            {
                if (_kattributes == null)
                    _kattributes = new KntKAttributeService(_repository);
                return _kattributes;
            }
        }

        private IKntSystemValuesService _systemValues;
        public IKntSystemValuesService SystemValues
        {
            get
            {
                if (_systemValues == null)
                    _systemValues = new KntSystemValuesService(_repository);
                return _systemValues;
            }
        }

        private IKntFolderService _folders;
        public IKntFolderService Folders
        {
            get
            {
                if (_folders == null)
                    _folders = new KntFolderService(_repository);
                return _folders;
            }
        }

        private IKntNoteService _notes;
        public IKntNoteService Notes
        {
            get
            {
                if (_notes == null)
                    _notes = new KntNoteService(_repository);
                return _notes;
            }
        }

        private IKntNoteTypeService _noteTypes;
        public IKntNoteTypeService NoteTypes
        {
            get
            {
                if (_noteTypes == null)
                    _noteTypes = new KntNoteTypeService(_repository);
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
                    return await Task.FromResult<bool>(false);

                if (!string.IsNullOrEmpty(newOwner))
                {
                    var resGetU = await Users.GetByUserNameAsync("owner");
                    if (resGetU.IsValid)
                    {
                        resGetU.Entity.UserName = newOwner;
                        var resUpdateU = await Users.SaveAsync(resGetU.Entity);
                        if(!resUpdateU.IsValid)
                            return await Task.FromResult<bool>(false);
                    }
                    else
                        return await Task.FromResult<bool>(false);
                }
            }
            catch (Exception)
            {
                return await Task.FromResult<bool>(false);
            }
            return await Task.FromResult<bool>(true);
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
