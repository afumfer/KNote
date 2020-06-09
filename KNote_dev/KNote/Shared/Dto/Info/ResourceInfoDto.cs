using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto.Info
{
    public class ResourceInfoDto : KntModelBase
    {
        public Guid ResourceId { get; set; }
        public string Name { get; set; }
        public string Container { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public string FileType { get; set; }
        public bool ContentInDB { get; set; }
        public byte[] ContentArrayBytes { get; set; }
        public Guid NoteId { get; set; }
    }
}
