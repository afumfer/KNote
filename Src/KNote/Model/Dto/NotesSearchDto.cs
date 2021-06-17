using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KNote.Model.Dto
{
    public class NotesSearchDto : PaginationBase
    {
        public string TextSearch { get; set; }

        public bool SearchInDescription { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = base.Validate(validationContext);
        
            return results;
        }
    }
}
