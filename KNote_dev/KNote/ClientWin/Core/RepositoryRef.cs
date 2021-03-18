using KNote.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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


        // TODO: thinking ....

        //private string _dataSource;
        //[Required(ErrorMessage = KMSG)]
        //public string DataSource
        //{
        //    get { return _dataSource; }
        //    set
        //    {
        //        if (_dataSource != value)
        //        {
        //            _dataSource = value;
        //            OnPropertyChanged("DataSource");
        //        }
        //    }
        //}

        //private string _dataBase;
        //[Required(ErrorMessage = KMSG)]
        //public string DataBase
        //{
        //    get { return _dataBase; }
        //    set
        //    {
        //        if (_dataBase != value)
        //        {
        //            _dataBase = value;
        //            OnPropertyChanged("DataBase");
        //        }
        //    }
        //}


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


            return results;
        }
    }
}
