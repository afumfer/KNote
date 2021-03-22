using KNote.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.ClientWin.Core
{
    public class RepositoryRef: DtoModelBase
    {
        private string _alias;
        [Required(ErrorMessage = KMSG)]        
        public string Alias
        {
            get { return _alias; }
            set
            {
                if (_alias != value)
                {
                    _alias = value;
                    OnPropertyChanged("Alias");
                }
            }
        }

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
        
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // ---
            // Capture the validations implemented with attributes.
            // TODO: (Esta sección se puede resolver por medio de reflexión).
            // ---

            Validator.TryValidateProperty(this.Alias,
               new ValidationContext(this, null, null) { MemberName = "Alias" },
               results);

            Validator.TryValidateProperty(this.ConnectionString,
               new ValidationContext(this, null, null) { MemberName = "ConnectionString" },
               results);

            Validator.TryValidateProperty(this.Provider,
               new ValidationContext(this, null, null) { MemberName = "Provider" },
               results);

            Validator.TryValidateProperty(this.Orm,
               new ValidationContext(this, null, null) { MemberName = "Orm" },
               results);

            // ---
            // Specific validations
            // ----

            if(Provider != "Microsoft.Data.Sqlite" && Provider != "Microsoft.Data.SqlClient")
            {
                results.Add(new ValidationResult
                 ("KMSG: Provider is invalid. (Supported providers are Microsoft.Data.SqlClient or Microsoft.Data.Sqlite."
                 , new[] { "ConnectionString" }));
            }

            if(Orm != "Dapper" && Orm != "EntityFramework")
            {
                results.Add(new ValidationResult
                 ("KMSG: ORM is invalid. (Supported ORMs are Dapper or EntityFramework."
                 , new[] { "ConnectionString" }));
            }

            var connProperties = GetConnectionProperties();
            if (Provider == "Microsoft.Data.Sqlite")
            {
                // example connection "Data Source=D:\xx\MySqliteDataBase.db"              
                var directory = Path.GetDirectoryName(connProperties["Data Source"]);
                var dataBase = Path.GetFileName(connProperties["Data Source"]);
                if(string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(dataBase))
                {
                    results.Add(new ValidationResult
                     ("KMSG: Database directory and file name cannot be empty."
                     , new[] { "ConnectionString" }));
                }

                if (!Directory.Exists(directory))
                {
                    results.Add(new ValidationResult
                     ("KMSG: Database directory does not exist."
                     , new[] { "ConnectionString" }));
                }
            }
            else if (Provider == "Microsoft.Data.SqlClient")
            {
                // example "Data Source=.\sqlexpress;Initial Catalog=MyDataBase;Trusted_Connection=True;Connection Timeout=60;MultipleActiveResultSets=true;
                var sqlServer = connProperties["Data Source"];
                var dataBase = connProperties["Initial Catalog"];
                if (string.IsNullOrEmpty(sqlServer) || string.IsNullOrEmpty(dataBase))
                {
                    results.Add(new ValidationResult
                     ("KMSG: Database and sql server name cannot be empty."
                     , new[] { "ConnectionString" }));
                }
            }

            return results;
        }
    }
}
