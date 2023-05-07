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
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KNote.Repository.EntityFramework;

public class KntRepository : IKntRepository
{
    #region Protected fields
    
    private readonly RepositoryRef _repositoryRef;
    
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

    public KntRepository(RepositoryRef repositoryRef)
    {            
        _repositoryRef = repositoryRef;           
    }

    public KntRepository(KntDbContext singletonContext, RepositoryRef repositoryRef) : this(repositoryRef)
    {            
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
                    _noteTypes = new KntNoteTypeRepository(Context, _repositoryRef);
                else 
                    _noteTypes = new KntNoteTypeRepository(_repositoryRef);
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
                    _systemValues = new KntSystemValuesRepository(Context, _repositoryRef);
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
                if (_context != null)
                    _folders = new KntFolderRepository(Context, _repositoryRef);
                else
                    _folders = new KntFolderRepository(_repositoryRef);
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
                    _attributes = new KntKAttributeRepository(Context, _repositoryRef);
                else
                    _attributes = new KntKAttributeRepository(_repositoryRef);
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
                    _notes = new KntNoteRepository(Context, _repositoryRef);
                else
                    _notes = new KntNoteRepository(_repositoryRef);
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
                    _users = new KntUserRepository(Context, _repositoryRef);
                else
                    _users = new KntUserRepository(_repositoryRef);
            return _users;
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
            var optionsBuilder = new DbContextOptionsBuilder<KntDbContext>();

            if (_repositoryRef.Provider == "Microsoft.Data.SqlClient")
                optionsBuilder.UseSqlServer(_repositoryRef.ConnectionString);
            else if (_repositoryRef.Provider == "Microsoft.Data.Sqlite")
            {
                optionsBuilder.UseSqlite(_repositoryRef.ConnectionString);
                optionsBuilder.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
            }
            else
                return false;

            var dbContext = new KntDbContext(optionsBuilder.Options, false);
            var systemValues = new KntSystemValuesRepository(dbContext, _repositoryRef);
            var res = await systemValues.GetAllAsync();
            if (!res.IsValid)
                return false;
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

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

}
