using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using KNote.Model;

namespace KNote.Model.Entities
{
    public class SystemValue : ModelBase
    {
        #region Constructor

        public SystemValue() : base() { }
        
        #endregion

        #region Property definitions

        private Guid _systemValueId;
        [Key]
        public Guid SystemValueId
        {
            get { return _systemValueId; }
            set
            {
                if (_systemValueId != value)
                {
                    _systemValueId = value;
                    OnPropertyChanged("SystemValueId");
                }
            }
        }

        private string _scope;
        [Required(ErrorMessage = "KMSG: La clave es requerida")]
        [MaxLength(256)]
        public string Scope
        {
            get { return _scope; }
            set
            {
                if (_scope != value)
                {
                    _scope = value;
                    OnPropertyChanged("Scope");
                }
            }
        }

        private string _key;
        [Required(ErrorMessage = "KMSG: La clave es requerida")]
        [MaxLength(256)]        
        public string Key
        {
            get { return _key; }
            set
            {
                if (_key != value)
                {
                    _key = value;
                    OnPropertyChanged("Key");
                }
            }
        }

        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged("Value");
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

            Validator.TryValidateProperty(this.Key,
               new ValidationContext(this, null, null) { MemberName = "Key" },
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
