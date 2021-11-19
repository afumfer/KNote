using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

using KNote.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace KNote.Repository.EntityFramework.Entities
{
    public class Note : EntityModelBase
    {
        #region Constructor

        public Note() : base() { }
        
        #endregion

        #region Property definitions
        
        [Key]
        public Guid NoteId { get; set; }

        public int NoteNumber { get; set; }
        
        [Required (ErrorMessage= "KMSG: El asunto es requerido")]
        [MaxLength(1024)]        
        public string Topic { get; set; }

        [Required]
        public DateTime CreationDateTime { get; set; }
        
        [Required]
        public DateTime ModificationDateTime { get; set; }
        
        public string Description { get; set; }
        
        [MaxLength(64)]
        public string ContentType { get; set; }
        
        public string Script { get; set; }
        
        [MaxLength(256)]        
        public string InternalTags { get; set; }
        
        [MaxLength(1024)]
        public string Tags { get; set; }
        
        public int Priority { get; set; }
        
        public Guid FolderId { get; set; }
        
        public Guid? NoteTypeId { get; set; }

        #region Virtual - navigation properties
        
        public virtual Folder Folder { get; set; }
        
        public virtual NoteType NoteType { get; set; }
        
        public virtual List<NoteKAttribute> KAttributes { get; set; }
        
        public virtual List<Resource> Resources { get; set; }
        
        public virtual List<NoteTask> NoteTasks { get; set; }
        
        public virtual List<Window> Windows { get; set; }
        
        public virtual List<TraceNote> From { get; set; }

        public virtual List<TraceNote> To { get; set; }
        
        public virtual List<KMessage> KMessages { get; set; }

        #endregion

        #endregion

        #region Validations

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // ---
            // Capturar las validaciones implementadas vía atributos.
            // ---

            Validator.TryValidateProperty(this.Topic,
               new ValidationContext(this, null, null) { MemberName = "Topic" },
               results);

            Validator.TryValidateProperty(this.InternalTags,
               new ValidationContext(this, null, null) { MemberName = "InternalTags" },
               results);

            Validator.TryValidateProperty(this.Tags,
               new ValidationContext(this, null, null) { MemberName = "Tags" },
               results);

            //----
            // Validaciones específicas
            //----

            if (ModificationDateTime < CreationDateTime)
            {
                results.Add(new ValidationResult
                 ("KMSG: La fecha de modificación no puede ser mayor que la fecha de creación"
                 , new[] { "ModificationDateTime", "CreationDateTime" }));
            }

            // TODO: Validar NoteNumber

            // TODO: .....

            // ---
            // Retornar List<ValidationResult>()
            // ---           

            return results;
        }

        #endregion
    }
}
