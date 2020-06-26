using System;
using System.Collections.Generic;
using System.Text;

namespace KNote.Model.Dto
{
    public class PaginationDto
    {
        public int Page { get; set; } = 1;
        public int NumRecords { get; set; } = 25;

        // PageRequest
    }
}
