using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto
{
    public class ResourceInfoDto : KntModelBase
    {
        public Guid ResourceId { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public string FileMimeType { get; set; }
        public bool ContentInDB { get; set; }
        public byte[] ContentDB { get; set; }
        public Guid NoteId { get; set; }
    }
}
