using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

using KNote.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace KNote.Model.Entities
{
    public class NoteTask : EntityModelBase
    {
        #region Constructor 

        public NoteTask() : base() { }
        
        #endregion

        #region Property definitions
        
        [Key]
        public Guid NoteTaskId { get; set; }

        public Guid NoteId { get; set; }
        
        public Guid UserId { get; set; }
        
        public DateTime CreationDateTime { get; set; }

        public DateTime ModificationDateTime { get; set; }
        
        [Required(ErrorMessage = "KMSG: La descripción de la tarea es requerida")]        
        public string Description { get; set; }
        
        [MaxLength(1024)]
        public string Tags { get; set; }
        
        public int Priority { get; set; }
        
        public bool Resolved { get; set; }
        
        public double? EstimatedTime { get; set; }
        
        public double? SpentTime { get; set; }
        
        public double? DifficultyLevel { get; set; }
        
        public DateTime? ExpectedStartDate { get; set; }
        
        public DateTime? ExpectedEndDate { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }

        #region virtual - navigaion properties
        
        public virtual Note Note { get; set; }
        
        public virtual User User { get; set; }

        #endregion 

        #endregion

        #region Validations

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // ---
            // Capturar las validaciones implementadas vía atributos.
            // ---

            Validator.TryValidateProperty(this.Description,
               new ValidationContext(this, null, null) { MemberName = "Description" },
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

            if (ExpectedStartDate != null && ExpectedEndDate != null)
                if (ExpectedStartDate > ExpectedEndDate)
                {
                    results.Add(new ValidationResult
                     ("KMSG: La fecha de finalización no puede se superior a la fecha de inicio"
                     , new[] { "ExpectedStartDate", "ExpectedEndDate" }));
                }

            if (StartDate != null && EndDate != null)
                if (StartDate > EndDate)
                {
                    results.Add(new ValidationResult
                     ("KMSG: La fecha de inicio no puede ser superior a la fecha de resolución"
                     , new[] { "StartDate", "EndDate" }));
                }

            if (Resolved == true && EndDate == null)
            {
                results.Add(new ValidationResult
                 ("KMSG: Se el inidcador de resuelto está activo debe introducir una fecha de resolución"
                 , new[] { "Resolved", "EndDate" }));
            }

            // ---
            // Retornar List<ValidationResult>()
            // ---           

            return results;
        }


        #endregion
    }
}
