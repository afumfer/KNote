using System;
using System.Collections.Generic;
using System.Text;

namespace KNote.Shared.Dto
{
    public class NotesFilterDto
    {
        public int Page { get; set; } = 1;
        public int NumRecords { get; set; } = 25;
        public int TotalPages { get; set; } = 0;

        public PaginationDto Pagination
        {
            get { return new PaginationDto { Page = Page, NumRecords = NumRecords }; }
        }

        //public int NoteNumber { get; set; }
        public Guid? FolderId { get; set; }
        public string Topic { get; set; }
        public Guid? NoteTypeId { get; set; }
        public string Tags { get; set; }        
        public string Description { get; set; }

        public List<AtrFilterDto> AttributesFilter { get; set; } = new List<AtrFilterDto>();

        //public string Resource { get; set; }
        //public string Task { get; set; }
        //public string Message { get; set; }
    }

    public class AtrFilterDto
    {
        public string AtrKey { get; set; }
        public string AtrName { get; set; }
        public string Value { get; set; }
    }

}
