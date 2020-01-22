using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using System.Data.Common;
using System.ComponentModel;
using System.Linq.Expressions;

using KNote.Shared;
using KNote.DomainModel.Entities;
using KNote.DomainModel.Infrastructure;
using KNote.DomainModel.Repositories;

namespace KNote.DomainModel.Repositories
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

        private IRepository<KntDbContext, User> _users;
        public IRepository<KntDbContext, User> Users
        {
            get
            {
                if (_users == null)
                    _users = new Repository<KntDbContext, User>(_context, _throwKntException);                
                return _users;
            }
        }

        private IRepository<KntDbContext, Folder> _folders;
        public IRepository<KntDbContext, Folder> Folders
        {
            get
            {
                if (_folders == null)
                    _folders = new Repository<KntDbContext, Folder>(_context, _throwKntException);
                return _folders;
            }
        }

        private IRepository<KntDbContext, Note> _notes;
        public IRepository<KntDbContext, Note> Notes
        {
            get
            {
                if (_notes == null)
                    _notes = new Repository<KntDbContext, Note>(_context, _throwKntException);
                return _notes;
            }
        }
       
        private IRepository<KntDbContext, NoteKAttribute> _noteKAttributes;
        public IRepository<KntDbContext, NoteKAttribute> NoteKAttributes
        {
            get
            {
                if (_noteKAttributes == null)
                    _noteKAttributes = new Repository<KntDbContext, NoteKAttribute>(_context, _throwKntException);
                return _noteKAttributes;
            }
        }
   
        private IRepository<KntDbContext, NoteTask> _noteTask;
        public IRepository<KntDbContext, NoteTask> NoteTasks
        {
            get
            {
                if (_noteTask == null)
                    _noteTask = new Repository<KntDbContext, NoteTask>(_context, _throwKntException);
                return _noteTask;
            }
        }

        private IRepository<KntDbContext, Window> _windows;
        public IRepository<KntDbContext, Window> Windows
        {
            get
            {
                if (_windows == null)
                    _windows = new Repository<KntDbContext, Window>(_context, _throwKntException);
                return _windows;
            }
        }

        private IRepository<KntDbContext, Resource> _resources;
        public IRepository<KntDbContext, Resource> Resources
        {
            get
            {
                if (_resources == null)
                    _resources = new Repository<KntDbContext, Resource>(_context, _throwKntException);
                return _resources;
            }
        }

        private IRepository<KntDbContext, KAttribute> _attributes;
        public IRepository<KntDbContext, KAttribute> KAttributes
        {
            get
            {
                if (_attributes == null)
                    _attributes = new Repository<KntDbContext, KAttribute>(_context, _throwKntException);
                return _attributes;
            }
        }
        
        private IRepository<KntDbContext, KAttributeTabulatedValue> _attributeTabulatedValues;
        public IRepository<KntDbContext, KAttributeTabulatedValue> KAttributeTabulatedValues
        {
            get
            {
                if (_attributeTabulatedValues == null)
                    _attributeTabulatedValues = new Repository<KntDbContext, KAttributeTabulatedValue>(_context, _throwKntException);
                return _attributeTabulatedValues;
            }
        }

        private IRepository<KntDbContext, SystemValue> _systemValues;
        public IRepository<KntDbContext, SystemValue> SystemValues
        {
            get
            {
                if (_systemValues == null)
                    _systemValues = new Repository<KntDbContext, SystemValue>(_context, _throwKntException);
                return _systemValues;
            }
        }

        private IRepository<KntDbContext, TraceNote> _traceNotes;
        public IRepository<KntDbContext, TraceNote> TraceNotes
        {
            get
            {
                if (_traceNotes == null)
                    _traceNotes = new Repository<KntDbContext, TraceNote>(_context, _throwKntException);
                return _traceNotes;
            }
        }

        private IRepository<KntDbContext, KEvent> _kEvents;
        public IRepository<KntDbContext, KEvent> KEvents
        {
            get
            {
                if (_kEvents == null)
                    _kEvents = new Repository<KntDbContext, KEvent>(_context, _throwKntException);
                return _kEvents;
            }
        }

        private IRepository<KntDbContext, KMessage> _kMessages;
        public IRepository<KntDbContext, KMessage> KMessages
        {
            get
            {
                if (_kMessages == null)
                    _kMessages = new Repository<KntDbContext, KMessage>(_context, _throwKntException);
                return _kMessages;
            }
        }
        
        private IRepository<KntDbContext, KLog> _kLogs;
        public IRepository<KntDbContext, KLog> KLogs
        {
            get
            {
                if (_kLogs == null)
                    _kLogs = new Repository<KntDbContext, KLog>(_context, _throwKntException);
                return _kLogs;
            }
        }
      
        private IRepository<KntDbContext, NoteType> _noteTypes;
        public IRepository<KntDbContext, NoteType> NoteTypes
        {
            get
            {
                if (_noteTypes == null)
                    _noteTypes = new Repository<KntDbContext, NoteType>(_context, _throwKntException);
                return _noteTypes;
            }
        }
        
        private IRepository<KntDbContext, TraceNoteType> _traceNoteTypes;
        public IRepository<KntDbContext, TraceNoteType> TraceNoteTypes
        {
            get
            {
                if (_traceNoteTypes == null)
                    _traceNoteTypes = new Repository<KntDbContext, TraceNoteType>(_context, _throwKntException);
                return _traceNoteTypes;
            }
        }

        public void RefresDbContext()
        {
            try
            {
                // TODO: Esto no funciona en EntityFrameworkCore
                //var conn = DbProviderFactories.GetFactory(_strProvider).CreateConnection();
                //conn.ConnectionString = _strConn;
                //_context = new KntDbContext(conn);


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
            if (_traceNotes != null)
                _traceNotes.Dispose();
            if (_kEvents != null)
                _kEvents.Dispose();
            if (_kMessages != null)
                _kMessages.Dispose();
            if (_noteKAttributes != null)
                _noteKAttributes.Dispose();
            if (_noteTask != null)
                _noteTask.Dispose();
            if (_windows != null)
                _windows.Dispose();
            if (_resources != null)
                _resources.Dispose();
            if (_attributeTabulatedValues != null)
                _attributeTabulatedValues.Dispose();
            if (_kLogs != null)
                _kLogs.Dispose();
            if (_noteTypes != null)
                _noteTypes.Dispose();
            if (_traceNoteTypes != null)
                _traceNoteTypes.Dispose();

            if (_context != null)
                _context.Dispose();
        }

        #endregion 
    }
}
