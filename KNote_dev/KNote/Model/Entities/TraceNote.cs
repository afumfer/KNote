using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using KNote.Model;

namespace KNote.Model.Entities
{
    public class TraceNote : EntityModelBase
    {
        #region Constructor 

        public TraceNote() : base() { }
        
        #endregion

        #region Property definitions
        
        [Key]
        public Guid TraceNoteId { get; set; }

        public Guid FromId { get; set; }
        
        public Guid ToId { get; set; }
                
        public int Order { get; set; }
        
        public double Weight { get; set; }
        
        public Guid? TraceNoteTypeId { get; set; }

        #region Virtual - navigation properties
        
        [InverseProperty("From")]
        [ForeignKey("FromId")]
        public virtual Note From { get; set; }
        
        [InverseProperty("To")]
        [ForeignKey("ToId")]
        public virtual Note To { get; set; }
        
        public virtual TraceNoteType TraceNoteType { get; set; }

        #endregion

        #endregion

        #region Validations

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // ---
            // Capturar las validaciones implementadas vía atributos.
            // ---

            //Validator.TryValidateProperty(this.Key,
            //   new ValidationContext(this, null, null) { MemberName = "Key" },
            //   results);

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
