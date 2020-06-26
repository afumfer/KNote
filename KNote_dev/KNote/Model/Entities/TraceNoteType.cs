using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using KNote.Model;

namespace KNote.Model.Entities
{
    public class TraceNoteType : ModelBase
    {
        #region Constructor

        public TraceNoteType() : base() { }        

        #endregion

        #region Property definitions

        private Guid _traceNoteTypeId;
        [Key]
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

