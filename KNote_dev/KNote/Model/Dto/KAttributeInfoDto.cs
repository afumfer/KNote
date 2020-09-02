using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class KAttributeInfoDto : DtoModelBase
    {
        #region Property definitions

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

        private string _name;
        [Required(ErrorMessage = "* Attribute {0} is required ")]
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

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged("Description");
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

        public NoteTypeDto NoteTypeDto { get; set; }

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
