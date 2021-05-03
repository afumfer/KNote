using KNote.Model.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model
{
    public abstract class PaginationBase : DtoModelBase
    {
        public int Page { get; set; } = 1;
        public int NumRecords { get; set; } = 25;
        public int TotalPages { get; set; } = 0;

        public PageIdentifier PageIdentifier
        {
            get { return new PageIdentifier { Page = Page, NumRecords = NumRecords }; }
        }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

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

    public class PageIdentifier
    {
        public int Page { get; set; } = 1;
        public int NumRecords { get; set; } = 25;
    }

}
