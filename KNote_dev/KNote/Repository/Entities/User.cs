using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

using KNote.Model;

namespace KNote.Repository.Entities
{
    public class User : EntityModelBase
    {
        #region Constructor

        public User() : base() { }

        #endregion

        #region Property definitions
        
        [Key]
        public Guid UserId { get; set; }
        
        [Required(ErrorMessage = "KMSG: El nombre es requerido")]
        [MaxLength(32)]
        public string UserName { get; set; }
        
        [Required(ErrorMessage = "KMSG: El correo electrónico es requerido")]
        [MaxLength(256)]
        public string EMail { get; set; }
        
        [MaxLength(256)]
        public string FullName { get; set; }
        
        public string RoleDefinition { get; set; }
        
        public bool Disabled { get; set; }
        
        public byte[] PasswordHash { get; set; }
        
        public byte[] PasswordSalt { get; set; }

        #region Virtual - navigation properties 
        
        public virtual List<Window> Windows { get; set; }
        
        public virtual List<NoteTask> Tasks { get; set; }
        
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

            Validator.TryValidateProperty(this.UserName,
               new ValidationContext(this, null, null) { MemberName = "UserName" },
               results);

            Validator.TryValidateProperty(this.EMail,
               new ValidationContext(this, null, null) { MemberName = "EMail" },
               results);

            Validator.TryValidateProperty(this.FullName,
               new ValidationContext(this, null, null) { MemberName = "FullName" },
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
