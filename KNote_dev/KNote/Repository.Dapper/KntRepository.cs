using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.ComponentModel;
using System.Linq.Expressions;

using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Dapper;
using System.Data;

using KNote.Model;

namespace KNote.Repository.Dapper
{
    public class KntRepository : IKntRepository
    {
        #region Private/protected fields
        
        private bool _throwKntException;        
        private readonly RepositoryRef _repositoryRef;
        private readonly DbConnection _db;

        #endregion

        #region Constructor

        public KntRepository(RepositoryRef repositoryRef, bool throwKntException = false)
        {
            _throwKntException = throwKntException;
            _repositoryRef = repositoryRef;
        }

        public KntRepository(DbConnection singletonConnection, RepositoryRef repositoryRef, bool throwKntException = false)
        {
            _throwKntException = throwKntException;
            _db = singletonConnection;
            _repositoryRef = repositoryRef;
        }

        #endregion

        #region IKntRepository implementation         

        private IKntNoteTypeRepository _noteTypes;
        public IKntNoteTypeRepository NoteTypes
        {
            get
            {
                if (_noteTypes == null)
                    if(_db != null)
                        _noteTypes = new KntNoteTypeRepository(_db, _repositoryRef, _throwKntException);
                    else
                        _noteTypes = new KntNoteTypeRepository(_repositoryRef, _throwKntException);
                return _noteTypes;
            }
        }
        
        private IKntUserRepository _users;
        public IKntUserRepository Users
        {
            get
            {
                if (_users == null)
                    if (_db != null)
                        _users = new KntUserRepository(_db, _repositoryRef, _throwKntException);
                    else
                        _users = new KntUserRepository(_repositoryRef, _throwKntException);
                return _users;
            }
        }
        
        private IKntSystemValuesRepository _systemValues;
        public IKntSystemValuesRepository SystemValues
        {
            get
            {
                if (_systemValues == null)
                    if (_db != null)
                        _systemValues = new KntSystemValuesRepository(_db, _repositoryRef, _throwKntException);
                    else
                        _systemValues = new KntSystemValuesRepository(_repositoryRef, _throwKntException);
                return _systemValues;
            }
        }
        
        private IKntFolderRepository _folders;
        public IKntFolderRepository Folders
        {
            get
            {
                if (_folders == null)
                    if (_db != null)
                        _folders = new KntFolderRepository(_db, _repositoryRef, _throwKntException);
                    else
                        _folders = new KntFolderRepository(_repositoryRef, _throwKntException);
                return _folders;
            }
        }

        private IKntKAttributeRepository _kAttributes;
        public IKntKAttributeRepository KAttributes
        {
            get
            {
                if (_kAttributes == null)
                    if (_db != null)
                        _kAttributes = new KntKAttributeRepository(_db, _repositoryRef, _throwKntException);
                    else
                        _kAttributes = new KntKAttributeRepository(_repositoryRef, _throwKntException);
                return _kAttributes;
            }
        }

        private IKntNoteRepository _notes;
        public IKntNoteRepository Notes
        {
            get
            {
                if (_notes == null)
                    if (_db != null)
                        _notes = new KntNoteRepository(_db, _repositoryRef, _throwKntException);
                    else
                        _notes = new KntNoteRepository(_repositoryRef, _throwKntException);
                return _notes;
            }
        }

        public RepositoryRef RespositoryRef
        {
            get { return _repositoryRef; }
        }

        public async Task<bool> TestDbConnection()
        {
            try
            {                
                if (_repositoryRef.Provider == "Microsoft.Data.SqlClient")
                {
                    var db = new SqlConnection(_repositoryRef.ConnectionString);
                    var systemValues = new KntSystemValuesRepository(db, _repositoryRef, true);
                    var testValues = systemValues.GetAllAsync();
                }                    
                else if (_repositoryRef.Provider == "Microsoft.Data.Sqlite")
                {
                    SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
                    SqlMapper.AddTypeHandler(new GuidHandler());
                    SqlMapper.AddTypeHandler(new TimeSpanHandler());
                    var db = new SqliteConnection(_repositoryRef.ConnectionString);

                    var systemValues = new KntSystemValuesRepository(db, _repositoryRef, true);
                    var res = await systemValues.GetAllAsync();
                    if (!res.IsValid)
                        return false;
                }
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }


        #endregion

        #region IDisposable

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
            //if (_windows != null)
            //    _windows.Dispose();
            //if (_kLogs != null)
            //    _kLogs.Dispose();
            //if (_traceNoteTypes != null)
            //    _traceNoteTypes.Dispose();

            _users = null;
            _folders = null;
            _notes = null;
            _kAttributes = null;
            _systemValues = null;
            _noteTypes = null;

            if (_db != null)
                _db.Dispose();
        }

        #endregion

    }

    #region Sqlite personalization 

    abstract class SqliteTypeHandler<T> : SqlMapper.TypeHandler<T>
    {
        // Parameters are converted by Microsoft.Data.Sqlite
        public override void SetValue(IDbDataParameter parameter, T value)
            => parameter.Value = value;
    }

    class DateTimeOffsetHandler : SqliteTypeHandler<DateTimeOffset>
    {
        public override DateTimeOffset Parse(object value)
            => DateTimeOffset.Parse((string)value);
    }

    class GuidHandler : SqliteTypeHandler<Guid>
    {
        public override Guid Parse(object value)
            => Guid.Parse((string)value);
    }

    class TimeSpanHandler : SqliteTypeHandler<TimeSpan>
    {
        public override TimeSpan Parse(object value)
            => TimeSpan.Parse((string)value);
    }

    #endregion 
}
