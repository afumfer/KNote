using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KNote.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace KNote.Model.Entities
{
    public class NoteKAttribute : EntityModelBase
    {
        #region Constructor

        public NoteKAttribute() : base() { }
        
        #endregion

        #region Property definitions
                
        [Key]
        public Guid NoteKAttributeId { get; set; }

        public Guid NoteId { get; set; }
        
        public Guid KAttributeId { get; set; }
        
        public string Value { get; set; }

        #region Virtual - navigation properties

        public virtual Note Note { get; set; }
        
        public virtual KAttribute KAttribute { get; set; }

        #endregion 

        #endregion

        #region Validations

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // ---
            // Capturar las validaciones implementadas vía atributos.
            // ---            

            //Validator.TryValidateProperty(this.Name,
            //   new ValidationContext(this, null, null) { MemberName = "Name" },
            //   results);

            //----
            // Validaciones específicas
            //----
            
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
