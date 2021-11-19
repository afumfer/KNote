using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

using KNote.Model;

namespace KNote.Repository.EntityFramework.Entities
{
    public class Folder : EntityModelBase
    {
        #region Constructor

        public Folder() : base() {}
        
        #endregion

        #region Property definitions
        
        [Key]
        public Guid FolderId { get; set; }

        public int FolderNumber { get; set; }
        
        [Required]
        public DateTime CreationDateTime { get; set; }
        
        [Required]
        public DateTime ModificationDateTime { get; set; }
        
        [Required(ErrorMessage = "KMSG: Name required")]
        [MaxLength(256)]
        public string Name { get; set; }
        
        [MaxLength(1024)]
        public string Tags { get; set; }
        
        [MaxLength(512)]
        public string PathFolder { get; set; }
        
        public int Order { get; set; }
        
        [MaxLength(256)]
        public string OrderNotes { get; set; }
        
        public string Script { get; set; }
        
        public Guid? ParentId { get; set; }

        #region Virtual - navigation properties
        
        [ForeignKey("ParentId")]   // Este atributo no va bien al desplazarlo a fluent api, se debe quedar aquí. 
        public virtual Folder ParentFolder { get; set; }

        public virtual List<Folder> ChildsFolders { get; set; }
        
        public virtual List<Note> Notes { get; set; }

        #endregion 

        #endregion

        #region Validations

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // ---
            // Capturar las validaciones implementadas vía atributos.
            // TODO: (Esta sección se puede resolver por medio de reflexión).
            // ---

            Validator.TryValidateProperty(this.Name,
               new ValidationContext(this, null, null) { MemberName = "Name" },
               results);

            Validator.TryValidateProperty(this.PathFolder,
               new ValidationContext(this, null, null) { MemberName = "PathFolder" },
               results);

            // ---
            // Validaciones específicas
            // ----
           
            if (ModificationDateTime < CreationDateTime)
            {
                results.Add(new ValidationResult
                 ("KMSG: La fecha de modificación no puede ser mayor que la fecha de creación"
                 , new[] { "ModificationDateTime", "CreationDateTime" }));
            }

            // TODO: FolderNumber debe ser único


            return results;
        }

        #endregion
    }
}
