using System;
using System.Collections.Generic;
using System.Text;

namespace KNote.Shared.Dto
{
    public class KntFileInfo
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public string Type { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string FileBase64 { get; set; }
    }
}
