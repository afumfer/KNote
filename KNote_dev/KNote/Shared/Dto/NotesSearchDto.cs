using System;
using System.Collections.Generic;
using System.Text;

namespace KNote.Shared.Dto
{
    public class NotesSearchDto
    {
        public int Page { get; set; } = 1;
        public int NumRecords { get; set; } = 25;

        public PaginationDto Pagination
        {
            get { return new PaginationDto { Page = Page, NumRecords = NumRecords }; }
        }

        public string TextSearch { get; set; }
        
    }
}
