using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using KNote.Model;

namespace KNote.Model.Entities
{
    public class Resource : EntityModelBase
    {
        #region Constructor

        public Resource() : base() { }
        
        #endregion

        #region Property definitions
        
        [Key]
        public Guid ResourceId { get; set; }

        [Required(ErrorMessage = "KMSG: El nombre del recurso es requerido")]
        [MaxLength(1024)]        
        public string Name { get; set; }
        
        public string Container { get; set; }
        
        public string Description { get; set; }
        
        public int Order { get; set; }
        
        [MaxLength(64)]
        public string FileType { get; set; }
        
        public bool ContentInDB { get; set; }
        
        public byte[] ContentArrayBytes { get; set; }
        
        public Guid NoteId { get; set; }

        #region Virtual - navigation properties
        
        public virtual Note Note { get; set; }

        #endregion 

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

