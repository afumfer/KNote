﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.ComponentModel;
using System.Linq.Expressions;

namespace KNote.Repository.EntityFramework
{
    public class KntRepository : IKntRepository
    {
        #region Protected fields

        private bool _throwKntException;
        private readonly string _strConn;
        private readonly string _strProvider;
        
        #endregion

        #region Properties

        private readonly KntDbContext _context;
        protected KntDbContext Context
        {
            get 
            {
                return _context;
            }            
        }

        #endregion 

        #region Constructors

        public KntRepository(string strConn, string strProvider = "Microsoft.Data.SqlClient", bool throwKntException = false)
        {
            _throwKntException = throwKntException;
            _strConn = strConn;
            _strProvider = strProvider;            
        }

        public KntRepository(KntDbContext singletonContext, bool throwKntException = false)
        {
            _throwKntException = throwKntException;
            _context = singletonContext;            
        }

        #endregion

        #region IKntRepository


        private IKntNoteTypeRepository _noteTypes;
        public IKntNoteTypeRepository NoteTypes 
        {
            get
            {
                if (_noteTypes == null)
                    if(_context != null)
                        _noteTypes = new KntNoteTypeRepository(Context, _throwKntException);
                    else 
                        _noteTypes = new KntNoteTypeRepository(_strConn, _strProvider, _throwKntException);
                return _noteTypes;
                
            }
        }

        private IKntSystemValuesRepository _systemValues;
        public IKntSystemValuesRepository SystemValues
        {
            get
            {
                if (_systemValues == null)
                    if (_context != null)
                        _systemValues = new KntSystemValuesRepository(Context, _throwKntException);
                    else
                        _systemValues = new KntSystemValuesRepository(_strConn, _strProvider, _throwKntException);
                return _systemValues;
            }
        }
        
        private IKntFolderRepository _folders;
        public IKntFolderRepository Folders
        {
            get
            {
                if (_folders == null)
                    if (_context != null)
                        _folders = new KntFolderRepository(Context, _throwKntException);
                    else
                        _folders = new KntFolderRepository(_strConn, _strProvider, _throwKntException);
                return _folders;
            }
        }

        private IKntKAttributeRepository _attributes;
        public IKntKAttributeRepository KAttributes
        {
            get
            {
                if (_attributes == null)
                    if (_context != null)
                        _attributes = new KntKAttributeRepository(Context, _throwKntException);
                    else
                        _attributes = new KntKAttributeRepository(_strConn, _strProvider, _throwKntException);
                return _attributes;
            }
        }

        private IKntNoteRepository _notes;
        public IKntNoteRepository Notes
        {
            get
            {
                if (_notes == null)
                    if (_context != null)
                        _notes = new KntNoteRepository(Context, _throwKntException);
                    else
                        _notes = new KntNoteRepository(_strConn, _strProvider, _throwKntException);
                return _notes;
            }
        }

        private IKntUserRepository _users;
        public IKntUserRepository Users
        {
            get
            {
                if (_users == null)
                    if (_context != null)
                        _users = new KntUserRepository(Context, _throwKntException);
                    else
                        _users = new KntUserRepository(_strConn, _strProvider, _throwKntException);
                return _users;
            }
        }
        
        #region Pendiente ....

        // TODO: pendiente de re-implementar con patrón IKntRepositoryXxxxxx

        //private IGenericRepositoryEF<KntDbContext, KEvent> _kEvents;
        //public IGenericRepositoryEF<KntDbContext, KEvent> KEvents
        //{
        //    get
        //    {
        //        if (_kEvents == null)
        //            _kEvents = new GenericRepositoryEF<KntDbContext, KEvent>(_context, _throwKntException);
        //        return _kEvents;
        //    }
        //}

        //private IGenericRepositoryEF<KntDbContext, KMessage> _kMessages;
        //public IGenericRepositoryEF<KntDbContext, KMessage> KMessages
        //{
        //    get
        //    {
        //        if (_kMessages == null)
        //            _kMessages = new GenericRepositoryEF<KntDbContext, KMessage>(_context, _throwKntException);
        //        return _kMessages;
        //    }
        //}

        //private IGenericRepositoryEF<KntDbContext, KLog> _kLogs;
        //public IGenericRepositoryEF<KntDbContext, KLog> KLogs
        //{
        //    get
        //    {
        //        if (_kLogs == null)
        //            _kLogs = new GenericRepositoryEF<KntDbContext, KLog>(_context, _throwKntException);
        //        return _kLogs;
        //    }
        //}

        //private IGenericRepositoryEF<KntDbContext, TraceNoteType> _traceNoteTypes;
        //public IGenericRepositoryEF<KntDbContext, TraceNoteType> TraceNoteTypes
        //{
        //    get
        //    {
        //        if (_traceNoteTypes == null)
        //            _traceNoteTypes = new GenericRepositoryEF<KntDbContext, TraceNoteType>(_context, _throwKntException);
        //        return _traceNoteTypes;
        //    }
        //}

        #endregion

        #endregion

        #region IDisposable members 

        public void Dispose()
        {
            // TODO: make this with reflection or clear code

            if (_users != null)
                _users.Dispose();
            if (_folders != null)
                _folders.Dispose();
            if (_notes != null)
                _notes.Dispose();
            if (_attributes != null)
                _attributes.Dispose();
            if (_systemValues != null)
                _systemValues.Dispose();
            if (_noteTypes != null)
                _noteTypes.Dispose();

            //if (_traceNotes != null)
            //    _traceNotes.Dispose();
            //if (_kEvents != null)
            //    _kEvents.Dispose();
            //if (_kMessages != null)
            //    _kMessages.Dispose();
            //if (_windows != null)
            //    _windows.Dispose();
            //if (_kLogs != null)
            //    _kLogs.Dispose();
            //if (_traceNoteTypes != null)
            //    _traceNoteTypes.Dispose();

            _users = null;
            _folders = null;
            _notes = null;
            _attributes = null;
            _systemValues = null;
            _noteTypes = null;

            if (Context != null)
                Context.Dispose();
        }

        #endregion

        #region  Private methods

        public KntDbContext GetKntDbContext(string connectionString, string provider, bool throwKntException = false)
        {
            var optionsBuilder = new DbContextOptionsBuilder<KntDbContext>();

            if (provider == "Microsoft.Data.SqlClient")                                    
                optionsBuilder.UseSqlServer(connectionString);                                    
            else if (provider == "Microsoft.Data.Sqlite")                
                optionsBuilder.UseSqlite(connectionString);                
            else
                throw new Exception("Data provider not suported (KntEx)");

            _throwKntException = throwKntException;

            return new KntDbContext(optionsBuilder.Options);            
        }

        #endregion

    }
}
