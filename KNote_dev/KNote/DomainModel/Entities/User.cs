using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

using KNote.Shared;

namespace KNote.DomainModel.Entities
{
    public class User : ModelBase
    {
        #region Constructor

        public User() : base() { }

        #endregion

        #region Property definitions

        private Guid _userId;
        [Key]
        public Guid UserId
        {
            get { return _userId; }
            set
            {
                if (_userId != value)
                {
                    _userId = value;
                    OnPropertyChanged("UserId");
                }
            }
        }

        private string _userName;        
        [Required(ErrorMessage = "KMSG: El nombre es requerido")]
        [MaxLength(32)]
        public string UserName
        {
            get { return _userName; }
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged("UserName");
                }
            }
        }

        private string _eMail;
        [Required(ErrorMessage = "KMSG: El correo electrónico es requerido")]
        [MaxLength(256)]
        public string EMail
        {
            get { return _eMail; }
            set
            {
                if (_eMail != value)
                {
                    _eMail = value;
                    OnPropertyChanged("EMail");
                }
            }
        }

        private string _fullName;
        [MaxLength(256)]
        public string FullName
        {
            get { return _fullName; }
            set
            {
                if (_fullName != value)
                {
                    _fullName = value;
                    OnPropertyChanged("FullName");
                }
            }
        }

        private string _roleDefinition;        
        public string RoleDefinition
        {
            get { return _roleDefinition; }
            set
            {
                if (_roleDefinition != value)
                {
                    _roleDefinition = value;
                    OnPropertyChanged("RoleDefinition");
                }
            }
        }

        private bool _disabled;
        public bool Disabled
        {
            get { return _disabled; }
            set
            {
                if (_disabled != value)
                {
                    _disabled = value;
                    OnPropertyChanged("Disabled");
                }
            }
        }

        private byte[] _passwordHash;
        public byte[] PasswordHash
        {
            get { return _passwordHash; }
            set
            {
                if (_passwordHash != value)
                {
                    _passwordHash = value;
                    OnPropertyChanged("PasswordHash");
                }
            }
        }

        private byte[] _passwordSalt;
        public byte[] PasswordSalt
        {
            get { return _passwordSalt; }
            set
            {
                if (_passwordSalt != value)
                {
                    _passwordSalt = value;
                    OnPropertyChanged("PasswordSalt");
                }
            }
        }

        #region Virtual - navigation properties 

        private List<Window> _windows;
        public virtual List<Window> Windows
        {
            get { return _windows; }
            set
            {
                if (_windows != value)
                {
                    _windows = value;
                    OnPropertyChanged("Windows");
                }
            }
        }

        private List<NoteTask> _tasks;
        public virtual List<NoteTask> Tasks
        {
            get { return _tasks; }
            set
            {
                if (_tasks != value)
                {
                    _tasks = value;
                    OnPropertyChanged("Tasks");
                }
            }
        }

        private List<KMessage> _kMessages;
        public virtual List<KMessage> KMessages
        {
            get { return _kMessages; }
            set
            {
                if (_kMessages != value)
                {
                    _kMessages = value;
                    OnPropertyChanged("Messages");
                }
            }
        }

        #endregion 

        #endregion

        #region Validations

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            
            // ---
            // Capturar las validaciones implementadas vía atributos.
            // ---            

            Validator.TryValidateProperty(this.UserName,
               new ValidationContext(this, null, null) { MemberName = "UserName" },
               results);

            Validator.TryValidateProperty(this.EMail,
               new ValidationContext(this, null, null) { MemberName = "EMail" },
               results);

            Validator.TryValidateProperty(this.FullName,
               new ValidationContext(this, null, null) { MemberName = "FullName" },
               results);

            //----
            // Validaciones específicas
            //----

            // ---- Ejemplo
            //if (ModificationDateTime < CreationDateTime)
            //{
            //    results.Add(new ValidationResult
            //     ("KMSG: La fecha de modificación no puede ser mayor que la fecha de creación"
            //     , new[] { "ModificationDateTime", "CreationDateTime" }));
            //}

            // ---
            // Retornar List<ValidationResult>()
            // ---           

            return results;
        }


        #endregion
    }
}
