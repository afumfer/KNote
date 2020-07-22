using System;
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

        private IKntNoteRepository _notes;
        public IKntNoteRepository Notes
        {
            get
            {
                if (_notes == null)
                    _notes = new KntNoteRepository(_context, _throwKntException);
                return _notes;
            }
        }

        private IKntUserRepository _users;
        public IKntUserRepository Users
        {
            get
            {
                if (_users == null)
                    _users = new KntUserRepository(_context, _throwKntException);
                return _users;
            }
        }

        // !!!!!!!!!!!!!!!!!!!!!!!!!!

        //private IGenericRepositoryEF<KntDbContext, User> _users;
        //public IGenericRepositoryEF<KntDbContext, User> Users
        //{
        //    get
        //    {
        //        if (_users == null)
        //            _users = new GenericRepositoryEF<KntDbContext, User>(_context, _throwKntException);                
        //        return _users;
        //    }
        //}

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
            //if (_traceNotes != null)
            //    _traceNotes.Dispose();
            //if (_kEvents != null)
            //    _kEvents.Dispose();
            //if (_kMessages != null)
            //    _kMessages.Dispose();
            //if (_noteKAttributes != null)
            //    _noteKAttributes.Dispose();
            //if (_noteTask != null)
            //    _noteTask.Dispose();
            //if (_windows != null)
            //    _windows.Dispose();
            //if (_resources != null)
            //    _resources.Dispose();
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
                if (_strProvider == "System.Data.SqlClient")
                {
                    var optionsBuilder = new DbContextOptionsBuilder<KntDbContext>();
                    optionsBuilder.UseSqlServer(_strConn);
                    _context = new KntDbContext(optionsBuilder.Options);
                }
                else
                    throw new Exception("Data provider not suported (KntEx)");

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
