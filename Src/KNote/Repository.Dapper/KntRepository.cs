using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Dapper;
using KNote.Model;

namespace KNote.Repository.Dapper;

public class KntRepository : IKntRepository
{
    #region Private/protected fields
                
    private readonly RepositoryRef _repositoryRef;
    private readonly DbConnection _db;

    #endregion

    #region Constructor

    public KntRepository(RepositoryRef repositoryRef)
    {            
        _repositoryRef = repositoryRef;
    }

    public KntRepository(DbConnection singletonConnection, RepositoryRef repositoryRef)
        :this(repositoryRef)
    {            
        _db = singletonConnection;            
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
                    _noteTypes = new KntNoteTypeRepository(_db, _repositoryRef);
                else
                    _noteTypes = new KntNoteTypeRepository(_repositoryRef);
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
                    _users = new KntUserRepository(_db, _repositoryRef);
                else
                    _users = new KntUserRepository(_repositoryRef);
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
                    _systemValues = new KntSystemValuesRepository(_db, _repositoryRef);
                else
                    _systemValues = new KntSystemValuesRepository(_repositoryRef);
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
                    _folders = new KntFolderRepository(_db, _repositoryRef);
                else
                    _folders = new KntFolderRepository(_repositoryRef);
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
                    _kAttributes = new KntKAttributeRepository(_db, _repositoryRef);
                else
                    _kAttributes = new KntKAttributeRepository(_repositoryRef);
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
                    _notes = new KntNoteRepository(_db, _repositoryRef);
                else
                    _notes = new KntNoteRepository(_repositoryRef);
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
                var systemValues = new KntSystemValuesRepository(db, _repositoryRef);
                var testValues = systemValues.GetAllAsync();
            }                    
            else if (_repositoryRef.Provider == "Microsoft.Data.Sqlite")
            {
                SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
                SqlMapper.AddTypeHandler(new GuidHandler());
                SqlMapper.AddTypeHandler(new TimeSpanHandler());
                var db = new SqliteConnection(_repositoryRef.ConnectionString);

                var systemValues = new KntSystemValuesRepository(db, _repositoryRef);
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

