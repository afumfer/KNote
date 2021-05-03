using System;
using System.Collections.Generic;
using System.Text;

namespace KNote.Model.Dto
{
    public class KntFileInfo
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public string Type { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string ContentBase64 { get; set; }
    }
}
