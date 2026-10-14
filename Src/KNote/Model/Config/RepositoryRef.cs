using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Xml.Serialization;

namespace KNote.Model;

// The repository's name and id are not configuration: they live in its own database (SystemValues
// DB_REPOSITORYNAME/DB_REPOSITORYID, see KntSchemaUpdater) and are filled in here when it is opened
// (ServiceRef), so they are never written to ClientWin's KNoteData.config.
public class RepositoryRef : SmartModelDtoBase
{
    private string _name;
    [XmlIgnore]
    public string Name
    {
        get { return _name; }
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
    }

    private Guid _repositoryId;
    [XmlIgnore]
    public Guid RepositoryId
    {
        get { return _repositoryId; }
        set
        {
            if (_repositoryId != value)
            {
                _repositoryId = value;
                OnPropertyChanged("RepositoryId");
            }
        }
    }

    // LEGACY(repository-alias): remove, with ShouldSerializeLegacyAlias, once every installation has opened
    // its repositories with a version that keeps the name in the database (KntSchemaUpdater revision 5).
    // The <Alias> of configuration files written before the name moved to the database. It is the name
    // given to the database when it gets one (KntSchemaUpdater), and it is written back only while the
    // repository hasn't been opened yet (ShouldSerializeLegacyAlias), so a repository that can't be opened
    // for a while keeps it until it can.
    private string _legacyAlias;
    [XmlElement("Alias")]
    public string LegacyAlias
    {
        get { return _legacyAlias; }
        set
        {
            _legacyAlias = value;
            if (string.IsNullOrEmpty(_name))
                _name = value;
        }
    }

    public bool ShouldSerializeLegacyAlias() => RepositoryId == Guid.Empty && !string.IsNullOrEmpty(_legacyAlias);

    private string _connectionString;
    [Required(ErrorMessage = KMSG)]
    public string ConnectionString
    {
        get { return _connectionString; }
        set
        {
            if (_connectionString != value)
            {
                _connectionString = value;
                OnPropertyChanged("ConnectionString");
            }
        }
    }

    private string _provider;
    [Required(ErrorMessage = KMSG)]
    public string Provider
    {
        get { return _provider; }
        set
        {
            if (_provider != value)
            {
                _provider = value;
                OnPropertyChanged("Provider");
            }
        }
    }

    private string _orm;
    [Required(ErrorMessage = KMSG)]
    public string Orm
    {
        get { return _orm; }
        set
        {
            if (_orm != value)
            {
                _orm = value;
                OnPropertyChanged("Orm");
            }
        }
    }

    private string _resourcesContainer;
    [Required(ErrorMessage = "Resources continer name is required.")]
    public string ResourcesContainer
    {
        get { return _resourcesContainer; }
        set
        {
            if (_resourcesContainer != value)
            {
                _resourcesContainer = value;
                OnPropertyChanged("ResourcesContainer");
            }
        }
    }

    private string _resourcesContainerRootPath;
    [Required(ErrorMessage = "Resources continer root folder is required.")]
    public string ResourcesContainerRootPath
    {
        get { return _resourcesContainerRootPath; }
        set
        {
            if (_resourcesContainerRootPath != value)
            {
                _resourcesContainerRootPath = value;
                OnPropertyChanged("ResourcesContainerRootPath");
            }
        }
    }

    private string _resourcesContainerRootUrl;
    [Required(ErrorMessage = "Resources continer URL is required.")]
    public string ResourcesContainerRootUrl
    {
        get { return _resourcesContainerRootUrl; }
        set
        {
            if (_resourcesContainerRootUrl != value)
            {
                _resourcesContainerRootUrl = value;
                OnPropertyChanged("ResourcesContainerRootUrl");
            }
        }
    }

    private bool _resourceContentInDB;
    public bool ResourceContentInDB
    {
        get { return _resourceContentInDB; }
        set
        {
            if (_resourceContentInDB != value)
            {
                _resourceContentInDB = value;
                OnPropertyChanged("ResourceContentInDB");
            }
        }
    }

    public Dictionary<string, string> GetConnectionProperties()
    {
        try
        {
            var connectValues = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(ConnectionString))
                return connectValues;

            var arrayValues = ConnectionString.Split(';');

            foreach (var strCon in arrayValues)
            {
                if (!string.IsNullOrEmpty(strCon))
                {
                    var keyValue = strCon.Trim().Split("=");
                    connectValues.Add(keyValue[0], keyValue[1]);
                }
            }

            return connectValues;
        }
        catch (Exception)
        {
            throw;
        }
    }

    // Name for a database that has none yet and no alias to take it from: the database's own name (the
    // Sqlite file without its extension, or the SQL Server database).
    public string GetDefaultName()
    {
        try
        {
            var properties = new Dictionary<string, string>(GetConnectionProperties(), StringComparer.OrdinalIgnoreCase);

            string name = null;
            if (Provider == "Microsoft.Data.Sqlite" && properties.TryGetValue("Data Source", out var dataSource))
                name = Path.GetFileNameWithoutExtension(dataSource?.Trim());
            else if (properties.TryGetValue("Initial Catalog", out var catalog) || properties.TryGetValue("Database", out catalog))
                name = catalog?.Trim();

            if (!string.IsNullOrEmpty(name))
                return name;
        }
        catch (Exception)
        {
            // A connection string GetConnectionProperties can't split: fall back to the generic name.
        }

        return $"{KntConst.AppName} repository";
    }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        // ---
        // Capture the validations implemented with attributes.
        // TODO: (Esta sección se puede resolver por medio de reflexión).
        // ---

        Validator.TryValidateProperty(this.ConnectionString,
           new ValidationContext(this, null, null) { MemberName = "ConnectionString" },
           results);

        Validator.TryValidateProperty(this.Provider,
           new ValidationContext(this, null, null) { MemberName = "Provider" },
           results);

        Validator.TryValidateProperty(this.Orm,
           new ValidationContext(this, null, null) { MemberName = "Orm" },
           results);

        Validator.TryValidateProperty(this.ResourcesContainer,
           new ValidationContext(this, null, null) { MemberName = "ResourcesContainer" },
           results);

        Validator.TryValidateProperty(this.ResourcesContainerRootPath,
           new ValidationContext(this, null, null) { MemberName = "ResourcesContainerRootPath" },
           results);

        Validator.TryValidateProperty(this.ResourcesContainerRootUrl,
           new ValidationContext(this, null, null) { MemberName = "ResourcesContainerRootUrl" },
           results);

        // ---
        // Specific validations
        // ----

        if (Provider != "Microsoft.Data.Sqlite" && Provider != "Microsoft.Data.SqlClient")
        {
            results.Add(new ValidationResult
             ("Provider is invalid. (Supported providers are Microsoft.Data.SqlClient or Microsoft.Data.Sqlite."
             , new[] { "ConnectionString" }));
        }

        if (!Directory.Exists(ResourcesContainerRootPath))
        {
            results.Add(new ValidationResult
             ("Resources root path directory does not exist."
             , new[] { "ResourcesContainerRootPath" }));
        }

        if (Orm != "Dapper" && Orm != "EntityFramework")
        {
            results.Add(new ValidationResult
             ("ORM is invalid. (Supported ORMs are Dapper or EntityFramework."
             , new[] { "ConnectionString" }));
        }

        var connProperties = GetConnectionProperties();
        if (Provider == "Microsoft.Data.Sqlite")
        {
            var directory = Path.GetDirectoryName(connProperties["Data Source"]);
            var dataBase = Path.GetFileName(connProperties["Data Source"]);
            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(dataBase))
            {
                results.Add(new ValidationResult
                 ("Database directory and file name cannot be empty."
                 , new[] { "ConnectionString" }));
            }

            if (!Directory.Exists(directory))
            {
                results.Add(new ValidationResult
                 ("Database directory does not exist."
                 , new[] { "ConnectionString" }));
            }
        }
        else if (Provider == "Microsoft.Data.SqlClient")
        {
            var sqlServer = connProperties["Data Source"];
            var dataBase = connProperties["Initial Catalog"];
            if (string.IsNullOrEmpty(sqlServer) || string.IsNullOrEmpty(dataBase))
            {
                results.Add(new ValidationResult
                 ("Database and sql server name cannot be empty."
                 , new[] { "ConnectionString" }));
            }
        }

        return results;
    }
}
