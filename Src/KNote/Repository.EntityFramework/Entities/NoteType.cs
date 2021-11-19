using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using KNote.Model;

namespace KNote.Repository.EntityFramework.Entities
{
    public class NoteType : EntityModelBase
    {
        #region Constructor

        public NoteType() : base() { }
        
        #endregion

        #region Property definitions
        
        [Key]
        public Guid NoteTypeId { get; set; }

        [Required(ErrorMessage = "KMSG: El nombre del tipo es requerido")]
        [MaxLength(256)]        
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public Guid? ParenNoteTypeId { get; set; }

        #endregion

        #region Virtual - navigation properties
        
        [ForeignKey("ParenNoteTypeId")]   
        public virtual NoteType ParenNoteType { get; set; }

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

