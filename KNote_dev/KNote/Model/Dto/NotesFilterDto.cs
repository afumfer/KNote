﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KNote.Model.Dto
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
        private const string KMSG = "Attribute {0} is required. ";
        //public string AtrKey { get; set; }
        [Required(ErrorMessage = KMSG)]
        public Guid AtrId { get; set; }

        public string AtrName { get; set; }
        
        [Required(ErrorMessage = KMSG)]
        public string Value { get; set; }
    }

}
