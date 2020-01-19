using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto
{
    public class NoteKAttributeDto : KntModelBase
    {
        public Guid NoteKAttributeId { get; set; }
        public Guid NoteId { get; set; }
        public Guid KAttributeId { get; set; }
        public string KAttributeName { get; set; }
        public string Value { get; set; }
    }
}