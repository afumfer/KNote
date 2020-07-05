using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using KNote.Model;

namespace KNote.Model.Entities
{
    public class KAttributeTabulatedValue : EntityModelBase
    {
        #region Constructor

        public KAttributeTabulatedValue() : base () { }
        
        #endregion

        #region Property definitions
        
        [Key]
        public Guid KAttributeTabulatedValueId { get; set; }

        public Guid KAttributeId { get; set; }
        
        [Required(ErrorMessage = "KMSG: El valor para el atributo es requerido")]
        public string Value { get; set; }
        
        public string Description { get; set; }
        
        public int Order { get; set; }

        #region Virtual - navigation properties
        
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
