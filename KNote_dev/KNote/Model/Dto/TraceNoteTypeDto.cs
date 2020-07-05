using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class TraceNoteTypeDto : DtoModelBase
    {
        //public Guid TraceNoteTypeId { get; set; }

        //[Required(ErrorMessage = "* Attribute {0} is required ")]
        //[MaxLength(256)]
        //public string Name { get; set; }

        //public string Description { get; set; }

        #region Property definitions

        private Guid _traceNoteTypeId;        
        public Guid TraceNoteTypeId
        {
            get { return _traceNoteTypeId; }
            set
            {
                if (_traceNoteTypeId != value)
                {
                    _traceNoteTypeId = value;
                    OnPropertyChanged("TraceNoteTypeId");
                }
            }
        }

        private string _name;
        [Required(ErrorMessage = "KMSG: El nombre del tipo es requerido")]
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