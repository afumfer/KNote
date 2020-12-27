using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class KAttributeTabulatedValueDto : DtoModelBase
    {
        #region Property definitions

        private Guid _kattributeTabulatedValueId;        
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

        private string _value;
        [Required(ErrorMessage = "* Attribute {0} is required ")]
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

        #endregion 

        #region Validations

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // ---
            // Capture the validations implemented with attributes.
            // ---

            Validator.TryValidateProperty(this.Value,
               new ValidationContext(this, null, null) { MemberName = "Value" },
               results);

            //----
            // Specific validations
            //----

            // ---- Ejemplo
            //if (ModificationDateTime < CreationDateTime)
            //{
            //    results.Add(new ValidationResult
            //     ("KMSG: The modification date cannot be greater than the creation date "
            //     , new[] { "ModificationDateTime", "CreationDateTime" }));
            //}

            // ---
            // Return List<ValidationResult>()
            // ---
            return results;
        }

        #endregion
    }
}
