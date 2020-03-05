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
        public string Value { get; set; }
        public string KAttributeName { get; set; }        
        public string Key { get; set; }        
        public EnumKAttributeDataType KAttributeDataType { get; set; }
        public bool RequiredValue { get; set; }
        public int Order { get; set; }
        public string Script { get; set; }
        public bool Disabled { get; set; }
    }
}