using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KNote.Model.Dto
{
    public class NotesSearchDto : DtoModelBase // PaginationBase
    {
        public string TextSearch { get; set; }

        public bool SearchInDescription { get; set; }
        
        public PaginationContext PaginationContext{ get; }

        public NotesSearchDto()
        {
            PaginationContext = new PaginationContext();
        }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            return results;
        }
    }

    // TODO: tmp class, refactor
    public class NotesSearchParam
    {
        public string TextSearch { get; set; }
        public int Page { get; set; }
    }
}
