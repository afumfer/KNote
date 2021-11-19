using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KNote.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace KNote.Repository.EntityFramework.Entities
{
    public class KEvent : EntityModelBase
    {
        #region Constructor

        public KEvent() : base() { }
        
        #endregion

        #region Property definitions

        [Key]
        public Guid KEventId { get; set; }
        
        public Guid? NoteScriptId { get; set; }
        
        public Guid? EntityId { get; set; }
        
        [MaxLength(64)]        
        public string EntityName { get; set; }
        
        [MaxLength(64)]
        public string PropertyName { get; set; }
        
        public string PropertyValue { get; set; }
        
        public EnumEventType EventType { get; set; }
        
        #region Virtual - navigation properties
        
        [ForeignKey("NoteScriptId")]
        public virtual Note NoteScript { get; set; }

        #endregion 

        #endregion

        #region Validations

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // ---
            // Capturar las validaciones implementadas vía atributos.
            // ---            

            Validator.TryValidateProperty(this.EntityName,
               new ValidationContext(this, null, null) { MemberName = "EntityName" },
               results);

            Validator.TryValidateProperty(this.PropertyName,
               new ValidationContext(this, null, null) { MemberName = "PropertyName" },
               results);

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

    public enum EnumEventType
    {        
        OnCreateActionDefault,
        OnSaveActionDefault,
        OnDeleteActionDefault,
        OnPropertyGetValueActionDefault,
        OnPropertyChangeActionDefault,
        OnCreateScriptExec,
        OnSaveScriptExec,
        OnDeleteScriptExec,
        OnPropertyGetValueScriptExec,
        OnPropertyChangeScriptExec
    }
}
