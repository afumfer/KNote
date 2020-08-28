using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.ComponentModel;
using System.Linq.Expressions;

using Microsoft.Data.SqlClient;

namespace KNote.Repository.Dapper
{
    public class KntRepository : IKntRepository
    {

        protected SqlConnection _db;
        protected bool _throwKntException;
        protected string _strConn;
        protected string _strProvider;

        public KntRepository(string strConn, string strProvider = "System.Data.SqlClient", bool throwKntException = false)
        {
            _throwKntException = throwKntException;
            _strConn = strConn;
            _strProvider = strProvider;

            RefresDbContext();
        }

        private IKntNoteTypeRepository _noteTypes;
        public IKntNoteTypeRepository NoteTypes
        {
            get
            {
                if (_noteTypes == null)
                    _noteTypes = new KntNoteTypeRepository(_db, _throwKntException);
                return _noteTypes;
            }
        }
        
        private IKntUserRepository _users;
        public IKntUserRepository Users
        {
            get
            {
                if (_users == null)
                    _users = new KntUserRepository(_db, _throwKntException);
                return _users;
            }
        }
        
        private IKntSystemValuesRepository _systemValues;
        public IKntSystemValuesRepository SystemValues
        {
            get
            {
                if (_systemValues == null)
                    _systemValues = new KntSystemValuesRepository(_db, _throwKntException);
                return _systemValues;
            }
        }
        
        private IKntFolderRepository _folders;
        public IKntFolderRepository Folders
        {
            get
            {
                if (_folders == null)
                    _folders = new KntFolderRepository(_db, _throwKntException);
                return _folders;
            }
        }

        private IKntKAttributeRepository _kAttributes;
        public IKntKAttributeRepository KAttributes
        {
            get
            {
                if (_kAttributes == null)
                    _kAttributes = new KntKAttributeRepository(_db, _throwKntException);
                return _kAttributes;
            }
        }

        private IKntNoteRepository _notes;
        public IKntNoteRepository Notes
        {
            get
            {
                if (_notes == null)
                    _notes = new KntNoteRepository(_db, _throwKntException);
                return _notes;
            }
        }
                    
        public void Dispose()
        {
            // TODO: make this with reflection or clear code

            if (_users != null)
                _users.Dispose();
            if (_folders != null)
                _folders.Dispose();
            if (_notes != null)
                _notes.Dispose();
            if (_kAttributes != null)
                _kAttributes.Dispose();
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
            //if (_traceNoteTypes != null)
            //    _traceNoteTypes.Dispose();

            if (_db != null)
                _db.Dispose();
        }

        public void RefresDbContext()
        {
            try
            {
                _db = new SqlConnection(_strConn);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
