using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.ComponentModel;
using System.Linq.Expressions;
using KNote.Model;
using KNote.Model.Entities;
using KNote.ServiceEF.Infrastructure;
using KNote.ServiceEF.Repositories;
using KNote.Model.Services;

namespace KNote.ServiceEF.Services
{
    public class KntService: IKntService, IDisposable
    {
        #region Fields

        protected KntRepository _repository;

        #endregion

        #region Constructors

        public KntService(string strConn, string strProvider = "System.Data.SqlClient", bool throwKntException = false)
        {            
            _repository = new KntRepository(strConn, strProvider, throwKntException);
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

        private IKntKMessageService _kMessages;
        public IKntKMessageService KMessages
        {
            get
            {
                if (_kMessages == null)
                    _kMessages = new KntKMessageService(_repository);
                return _kMessages;
            }
        }

        private IKntKEventService _kEvents;
        public IKntKEventService KEvents
        {
            get
            {
                if (_kEvents == null)
                    _kEvents = new KntKEventService(_repository);
                return _kEvents;
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
