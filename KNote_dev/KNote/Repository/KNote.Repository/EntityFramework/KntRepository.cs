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
using KNote.Repository.Entities;
using KNote.Repository.EntityFramework;
using KNote.Repository;

namespace KNote.Repository.EntityFramework
{
    public class KntRepository : IKntRepository
    {
        #region Protected fields

        protected KntDbContext _context;
        protected bool _throwKntException;
        protected string _strConn;
        protected string _strProvider;

        #endregion

        #region Constructors

        public KntRepository(string strConn, string strProvider = "System.Data.SqlClient", bool throwKntException = false)
        {
            _throwKntException = throwKntException;
            _strConn = strConn;
            _strProvider = strProvider;

            RefresDbContext();
        }

        #endregion

        #region IKntRepository


        private IKntNoteTypeRepository _noteTypes;
        public IKntNoteTypeRepository NoteTypes 
        {
            get
            {
                if (_noteTypes == null)
                    _noteTypes = new KntNoteTypeRepository(_context, _throwKntException);
                return _noteTypes;
            }
        }

        private IKntSystemValuesRepository _systemValues;
        public IKntSystemValuesRepository SystemValues
        {
            get
            {
                if (_systemValues == null)
                    _systemValues = new KntSystemValuesRepository(_context, _throwKntException);
                return _systemValues;
            }
        }
        
        private IKntFolderRepository _folders;
        public IKntFolderRepository Folders
        {
            get
            {
                if (_folders == null)
                    _folders = new KntFolderRepository(_context, _throwKntException);
                return _folders;
            }
        }

        private IKntKAttributeRepository _attributes;
        public IKntKAttributeRepository KAttributes
        {
            get
            {
                if (_attributes == null)
                    _attributes = new KntKAttributeRepository(_context, _throwKntException);
                return _attributes;
            }
        }

        // !!!!!!!!!!!!!!!!!!!!!!!!!!

        private IGenericRepositoryEF<KntDbContext, User> _users;
        public IGenericRepositoryEF<KntDbContext, User> Users
        {
            get
            {
                if (_users == null)
                    _users = new GenericRepositoryEF<KntDbContext, User>(_context, _throwKntException);                
                return _users;
            }
        }

        private IGenericRepositoryEF<KntDbContext, Note> _notes;
        public IGenericRepositoryEF<KntDbContext, Note> Notes
        {
            get
            {
                if (_notes == null)
                    _notes = new GenericRepositoryEF<KntDbContext, Note>(_context, _throwKntException);
                return _notes;
            }
        }
       
        private IGenericRepositoryEF<KntDbContext, NoteKAttribute> _noteKAttributes;
        public IGenericRepositoryEF<KntDbContext, NoteKAttribute> NoteKAttributes
        {
            get
            {
                if (_noteKAttributes == null)
                    _noteKAttributes = new GenericRepositoryEF<KntDbContext, NoteKAttribute>(_context, _throwKntException);
                return _noteKAttributes;
            }
        }
   
        private IGenericRepositoryEF<KntDbContext, NoteTask> _noteTask;
        public IGenericRepositoryEF<KntDbContext, NoteTask> NoteTasks
        {
            get
            {
                if (_noteTask == null)
                    _noteTask = new GenericRepositoryEF<KntDbContext, NoteTask>(_context, _throwKntException);
                return _noteTask;
            }
        }

        private IGenericRepositoryEF<KntDbContext, Resource> _resources;
        public IGenericRepositoryEF<KntDbContext, Resource> Resources
        {
            get
            {
                if (_resources == null)
                    _resources = new GenericRepositoryEF<KntDbContext, Resource>(_context, _throwKntException);
                return _resources;
            }
        }


        
        //private IGenericRepositoryEF<KntDbContext, KAttributeTabulatedValue> _attributeTabulatedValues;
        //public IGenericRepositoryEF<KntDbContext, KAttributeTabulatedValue> KAttributeTabulatedValues
        //{
        //    get
        //    {
        //        if (_attributeTabulatedValues == null)
        //            _attributeTabulatedValues = new GenericRepositoryEF<KntDbContext, KAttributeTabulatedValue>(_context, _throwKntException);
        //        return _attributeTabulatedValues;
        //    }
        //}



        //private IGenericRepositoryEF<KntDbContext, Window> _windows;
        //public IGenericRepositoryEF<KntDbContext, Window> Windows
        //{
        //    get
        //    {
        //        if (_windows == null)
        //            _windows = new GenericRepositoryEF<KntDbContext, Window>(_context, _throwKntException);
        //        return _windows;
        //    }
        //}

        //private IGenericRepositoryEF<KntDbContext, TraceNote> _traceNotes;
        //public IGenericRepositoryEF<KntDbContext, TraceNote> TraceNotes
        //{
        //    get
        //    {
        //        if (_traceNotes == null)
        //            _traceNotes = new GenericRepositoryEF<KntDbContext, TraceNote>(_context, _throwKntException);
        //        return _traceNotes;
        //    }
        //}

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

        #region IDisposable members 

        public void Dispose()
        {
            // TODO: make this with reflection 

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
            //if (_traceNotes != null)
            //    _traceNotes.Dispose();
            //if (_kEvents != null)
            //    _kEvents.Dispose();
            //if (_kMessages != null)
            //    _kMessages.Dispose();
            if (_noteKAttributes != null)
                _noteKAttributes.Dispose();
            if (_noteTask != null)
                _noteTask.Dispose();
            //if (_windows != null)
            //    _windows.Dispose();
            if (_resources != null)
                _resources.Dispose();
            //if (_attributeTabulatedValues != null)
            //    _attributeTabulatedValues.Dispose();
            //if (_kLogs != null)
            //    _kLogs.Dispose();
            if (_noteTypes != null)
                _noteTypes.Dispose();
            //if (_traceNoteTypes != null)
            //    _traceNoteTypes.Dispose();

            if (_context != null)
                _context.Dispose();
        }

        #endregion

        #region  Private methods

        public void RefresDbContext()
        {
            try
            {
                // Esto es para strProvider = "System.Data.SqlClient"                
                var optionsBuilder = new DbContextOptionsBuilder<KntDbContext>();
                optionsBuilder.UseSqlServer(_strConn);
                _context = new KntDbContext(optionsBuilder.Options);

                // TODO: Pendiente de establecer la conexión con Sqlite ...

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion 

    }
}
