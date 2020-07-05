using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class UserDto : DtoModelBase
    {
        //public Guid UserId { get; set; }
        //[Required(ErrorMessage = "* Attribute {0} is required ")]
        //[MaxLength(32)]
        //public string UserName { get; set; }
        //[Required(ErrorMessage = "* Attribute {0} is required ")]
        //[MaxLength(256)]
        //public string EMail { get; set; }
        //[Required(ErrorMessage = "* Attribute {0} is required ")]
        //[MaxLength(256)]
        //public string FullName { get; set; }        
        //public string RoleDefinition { get; set; }

        #region Property definitions

        private Guid _userId;        
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
        [Required(ErrorMessage = "* Attribute {0} is required")]
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
        [Required(ErrorMessage = "* Attribute {0} is required ")]
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
        [Required(ErrorMessage = "* Attribute {0} is required ")]
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
