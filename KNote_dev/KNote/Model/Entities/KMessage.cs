using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KNote.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace KNote.Model.Entities
{
    public class KMessage: EntityModelBase
    {
        #region Constructor

        public KMessage() : base() { }
        
        #endregion

        #region Property definitions
        
        [Key]
        public Guid KMessageId { get; set; }

        public Guid? UserId { get; set; }

        public Guid? NoteId { get; set; }
        
        public EnumActionType ActionType { get; set; }
        
        public EnumNotificationType NotificationType { get; set; }
        
        public EnumAlarmType AlarmType { get; set; }
        
        public bool Disabled { get; set; }
        
        public string Content { get; set; }
        
        public string Forward { get; set; }
       
        public bool? AlarmOk { get; set; }
        
        public bool? AlarmActivated { get; set; }
        
        public DateTime? AlarmDateTime { get; set; }
        
        public int? AlarmMinutes { get; set; }

        #region Virtual - navigation properties
       
        public virtual User User { get; set; }
        
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

            Validator.TryValidateProperty(this.Content,
               new ValidationContext(this, null, null) { MemberName = "Content" },
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

    #region Enums
    
    public enum EnumAlarmType
    {        
        Standard,
        Daily,
        Weekly,
        Monthly,
        Annual,
        InMinutes
    }
    
    public enum EnumNotificationType
    {        
        PrivateInfo,
        PostIt,
        Email
    }

    public enum EnumActionType
    {
        UserAlarm,
        NoteAlarm,
        UserMessage,
        NoteMessage,
        ScriptExecution
    }

    #endregion

}
