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

namespace KNote.Service.Services
{
    public class KntService: IKntService, IDisposable
    {
        #region Fields

        // !!! OJO, doble respositorio para el proceso de implementación de dapper y depuración. 
        // En la versión definitiva habrá una inyección de dependencia. 

        protected IKntRepository _repository;
        protected IKntRepository _repositoryDapper;

        #endregion

        #region Constructors

        public KntService(string strConn, string strProvider = "System.Data.SqlClient", bool throwKntException = false)
        {            
            _repository = new KntRepository(strConn, strProvider, throwKntException);
            _repositoryDapper = new DP.KntRepository(strConn, strProvider, throwKntException);
        }

        #endregion

        #region IKntService members

        private IKntUserService _users;
        public IKntUserService Users
        {
            get
            {
                if (_users == null)
                    //_users = new KntUserService(_repository);
                    _users = new KntUserService(_repositoryDapper);
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
                    //_kattributes = new KntKAttributeService(_repositoryDapper);
                return _kattributes;
            }
        }

        private IKntSystemValuesService _systemValues;
        public IKntSystemValuesService SystemValues
        {
            get
            {
                if (_systemValues == null)
                    //_systemValues = new KntSystemValuesService(_repository);
                    _systemValues = new KntSystemValuesService(_repositoryDapper);
                return _systemValues;
            }
        }

        private IKntFolderService _folders;
        public IKntFolderService Folders
        {
            get
            {
                if (_folders == null)
                    //_folders = new KntFolderService(_repository);
                    _folders = new KntFolderService(_repositoryDapper);
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
                    //_notes = new KntNoteService(_repositoryDapper);
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
                    _noteTypes = new KntNoteTypeService(_repositoryDapper);
                return _noteTypes;
            }
        }

        //private IKntKMessageService _kMessages;
        //public IKntKMessageService KMessages
        //{
        //    get
        //    {
        //        if (_kMessages == null)
        //            _kMessages = new KntKMessageService(_repository);
        //        return _kMessages;
        //    }
        //}

        //private IKntKEventService _kEvents;
        //public IKntKEventService KEvents
        //{
        //    get
        //    {
        //        if (_kEvents == null)
        //            _kEvents = new KntKEventService(_repository);
        //        return _kEvents;
        //    }
        //}


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
