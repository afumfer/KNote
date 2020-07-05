using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KNote.Model;

namespace KNote.Model.Entities
{
    public class KLog : EntityModelBase
    {
        #region Constructor

        public KLog() : base() { }
        
        #endregion

        #region Property definitions
        
        [Key]
        public Guid KLogId { get; set; }

        [Required(ErrorMessage = "KMSG: El Id de la entidad es requerido")]        
        public Guid EntityId { get; set; }
        
        [Required(ErrorMessage = "KMSG: El nombre de la entidad es requerido")]
        [MaxLength(64)]        
        public string EntityName { get; set; }
        
        [Required(ErrorMessage = "KMSG: La gecha del registro es requerida")]
        public DateTime RegistryDateTime { get; set; }
        
        [Required(ErrorMessage = "KMSG: El mensaje de registro es requerido")]
        public string RegistryMessage { get; set; }

        #endregion

        #region Validations

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // ---
            // Capturar las validaciones implementadas vía atributos.
            // ---

            Validator.TryValidateProperty(this.EntityId,
               new ValidationContext(this, null, null) { MemberName = "EntityId" },
               results);

            // TODO: Añadir aquí el resto de validaciones vía atributos ....

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
