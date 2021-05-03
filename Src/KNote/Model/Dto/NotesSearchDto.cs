using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KNote.Model.Dto
{
    public class NotesSearchDto : PaginationBase
    {
        public string TextSearch { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = base.Validate(validationContext);

            //var results = new List<ValidationResult>();

            // ---
            // Capture the validations implemented with attributes.
            // ---

            //Validator.TryValidateProperty(this.EntityId,
            //   new ValidationContext(this, null, null) { MemberName = "EntityId" },
            //   results);

            // TODO: Añadir aquí el resto de validaciones vía atributos ....

            //----
            // Specific validations
            //----

            // ---- Ejemplo
            //if (ModificationDateTime < CreationDateTime)
            //{
            //    results.Add(new ValidationResult
            //     ("KMSG: The modification date cannot be greater than the creation date "
            //     , new[] { "ModificationDateTime", "CreationDateTime" }));
            //}

            // ---
            // Return List<ValidationResult>()
            // ---           

            return results;
        }
    }
}
