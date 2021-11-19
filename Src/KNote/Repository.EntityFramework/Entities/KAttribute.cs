using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using KNote.Model;

namespace KNote.Repository.EntityFramework.Entities
{
    public class KAttribute : EntityModelBase
    {
        #region Constructor

        public KAttribute() : base() { }
        
        #endregion

        #region Property definitions
        
        [Key]
        public Guid KAttributeId { get; set; }

        [Required(ErrorMessage = "KMSG: La descripción del atributo es requerida")]
        [MaxLength(256)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public EnumKAttributeDataType KAttributeDataType { get; set; }
        
        public bool RequiredValue { get; set; }
        
        public int Order { get; set; }
        
        public string Script { get; set; }
        
        public bool Disabled { get; set; }
        
        public Guid? NoteTypeId { get; set; }

        #region Virtual - navitacion properties
        
        public virtual List<KAttributeTabulatedValue> KAttributeTabulatedValues { get; set; }
        
        public virtual List<NoteKAttribute> NoteAttributes { get; set; }
        
        public virtual NoteType NoteType { get; set; }

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
