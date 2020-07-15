using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KNote.Model;


namespace KNote.Repository.Entities
{
    public class Window : EntityModelBase
    {
        #region Constructor

        public Window() : base() { }
        
        #endregion

        #region Property definitions
        
        [Key]
        public Guid WindowId { get; set; }

        public Guid NoteId { get; set; }
        
        public Guid UserId { get; set; }

        // info del host / entorno del usuario         
        public string Host { get; set; }
        
        public bool Visible { get; set; }
        
        public bool AlwaysOnTop { get; set; }
        
        public int PosX { get; set; }
        
        public int PosY { get; set; }
        
        public int Width { get; set; }
        
        public int Height { get; set; }
        
        public string FontName { get; set; }
        
        public byte FontSize { get; set; }
        
        public bool FontBold { get; set; }

        public bool FontItalic { get; set; }
        
        public bool FontUnderline { get; set; }
        
        public bool FontStrikethru { get; set; }
        
        public int ForeColor { get; set; }
        
        public int TitleColor { get; set; }
        
        public int TextTitleColor { get; set; }
        
        public int NoteColor { get; set; }
        
        public int TextNoteColor { get; set; }

        #region Virtual - navigation properties
        
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

            //Validator.TryValidateProperty(this.Name,
            //   new ValidationContext(this, null, null) { MemberName = "Name" },
            //   results);

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
