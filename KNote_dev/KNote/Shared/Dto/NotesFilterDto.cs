﻿using System;
using System.Collections.Generic;
using System.Text;

namespace KNote.Shared.Dto
{
    public class NotesFilterDto
    {
        public int Page { get; set; } = 1;
        public int NumRecords { get; set; } = 10;

        public PaginationDto Pagination
        {
            get { return new PaginationDto { Page = Page, NumRecords = NumRecords }; }
        }

        public string Topic { get; set; }
        public Guid? FolderId { get; set; }
        public Guid? NoteTypeId { get; set; }
        public string Tags { get; set; }
        public string AttributeValue { get; set; }
    }
}