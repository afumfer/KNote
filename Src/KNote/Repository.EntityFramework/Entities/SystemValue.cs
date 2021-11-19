using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using KNote.Model;

namespace KNote.Repository.EntityFramework.Entities
{
    public class SystemValue : EntityModelBase
    {
        #region Constructor

        public SystemValue() : base() { }
        
        #endregion

        #region Property definitions
        
        [Key]
        public Guid SystemValueId { get; set; }

        [Required(ErrorMessage = "KMSG: La clave es requerida")]
        [MaxLength(256)]
        public string Scope { get; set; }
        
        [Required(ErrorMessage = "KMSG: La clave es requerida")]
        [MaxLength(256)]        
        public string Key { get; set; }
        
        public string Value { get; set; }

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
