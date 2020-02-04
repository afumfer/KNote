using System;
using System.Collections.Generic;
using System.Text;

namespace KNote.Shared.Dto
{
    public class Pagination
    {
        public int Page { get; set; } = 1;
        public int NumRecords { get; set; } = 10;

        // PageRequest
    }
}
