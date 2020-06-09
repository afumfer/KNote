using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto.Info
{ 
    public class NoteKAttributeInfoDto : KntModelBase
    {
        public Guid NoteKAttributeId { get; set; }
        public Guid NoteId { get; set; }
        public Guid KAttributeId { get; set; }
        public string Value { get; set; }
    }
}
