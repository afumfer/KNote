using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using KNote.Shared;

namespace KNote.DomainModel.Entities
{
    public class KAttributeTabulatedValue : ModelBase
    {
        #region Constructor

        public KAttributeTabulatedValue() : base () { }
        
        #endregion

        #region Property definitions

        private Guid _kattributeTabulatedValueId;
        [Key]
        public Guid KAttributeTabulatedValueId
        {
            get { return _kattributeTabulatedValueId; }
            set
            {
                if (_kattributeTabulatedValueId != value)
                {
                    _kattributeTabulatedValueId = value;
                    OnPropertyChanged("KAttributeTabulatedValueId");
                }
            }
        }

        private Guid _kattributeId;
        public Guid KAttributeId
        {
            get { return _kattributeId; }
            set
            {
                if (_kattributeId != value)
                {
                    _kattributeId = value;
                    OnPropertyChanged("KAttributeId");
                }
            }
        }

        private string _key;
        [Required(ErrorMessage = "KMSG: El valor para la clave del atributo es requerida")]
        [MaxLength(32)]        
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
        [Required(ErrorMessage = "KMSG: El valor para el atributo es requerido")]
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

        private int _order;
        public int Order
        {
            get { return _order; }
            set
            {
                if (_order != value)
                {
                    _order = value;
                    OnPropertyChanged("Order");
                }
            }
        }

        #region Virtual - navigation properties

        private KAttribute _kattribute;        
        public virtual KAttribute KAttribute
        {
            get { return _kattribute; }
            set
            {
                if (_kattribute != value)
                {
                    _kattribute = value;
                    OnPropertyChanged("KAttribute");
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

            Validator.TryValidateProperty(this.Value,
               new ValidationContext(this, null, null) { MemberName = "Value" },
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
