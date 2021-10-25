using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KNote.Model.Dto
{
    public class NotesSearchDto : SmartModelDtoBase // PaginationBase
    {
        public string TextSearch { get; set; }

        public bool SearchInDescription { get; set; }

        public PageIdentifier PageIdentifier { get; set; } = new PageIdentifier();

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
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
