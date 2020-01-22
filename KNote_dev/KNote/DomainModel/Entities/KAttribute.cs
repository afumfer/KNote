using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using KNote.Shared;

namespace KNote.DomainModel.Entities
{
    public class KAttribute : ModelBase
    {
        #region Constructor

        public KAttribute() : base() { }
        
        #endregion

        #region Property definitions

        private Guid _kattributeId;
        [Key]
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
        [Required(ErrorMessage = "KMSG: La clave del atributo requerida")]
        [MaxLength(16)]        
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

        private string _name;
        [Required(ErrorMessage = "KMSG: La descripción del atributo es requerida")]
        [MaxLength(256)]
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

        private EnumKAttributeDataType _kattributeDataType;
        public EnumKAttributeDataType KAttributeDataType
        {
            get { return _kattributeDataType; }
            set
            {
                if (_kattributeDataType != value)
                {
                    _kattributeDataType = value;
                    OnPropertyChanged("KAttributeDataType");
                }
            }
        }

        private bool _requiredValue;
        public bool RequiredValue
        {
            get { return _requiredValue; }
            set
            {
                if (_requiredValue != value)
                {
                    _requiredValue = value;
                    OnPropertyChanged("RequiredValue");
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

        private string _script;
        public string Script
        {
            get { return _script; }
            set
            {
                if (_script != value)
                {
                    _script = value;
                    OnPropertyChanged("Script");
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

        private Guid? _noteTypeId;
        public Guid? NoteTypeId
        {
            get { return _noteTypeId; }
            set
            {
                if (_noteTypeId != value)
                {
                    _noteTypeId = value;
                    OnPropertyChanged("NoteTypeId");
                }
            }
        }

        #region Virtual - navitacion properties

        private List<KAttributeTabulatedValue> _kattributeTabulatedValues;
        public virtual List<KAttributeTabulatedValue> KAttributeTabulatedValues
        {
            get { return _kattributeTabulatedValues; }
            set
            {
                if (_kattributeTabulatedValues != value)
                {
                    _kattributeTabulatedValues = value;
                    OnPropertyChanged("KAttributeTabulatedValues");
                }
            }
        }

        private List<NoteKAttribute> _noteAttributes;
        public virtual List<NoteKAttribute> NoteAttributes
        {
            get { return _noteAttributes; }
            set
            {
                if (_noteAttributes != value)
                {
                    _noteAttributes = value;
                    OnPropertyChanged("NoteAttributes");
                }
            }
        }

        private NoteType _noteType;        
        public virtual NoteType NoteType
        {
            get { return _noteType; }
            set
            {
                if (_noteType != value)
                {
                    _noteType = value;
                    OnPropertyChanged("NoteType");
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

            Validator.TryValidateProperty(this.Name,
               new ValidationContext(this, null, null) { MemberName = "Name" },
               results);

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

    #region Enums

    public enum EnumKAttributeDataType
    {
        dtTag,
        dtBool,
        dtInt,
        dtDouble,
        dtString,
        dtDateTime,
        dtTabulate,
    }

    #endregion
}
