using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto.Info
{
    public class KAttributeInfoDto : KntModelBase
    {
        public Guid KAttributeId { get; set; }        
        public string Name { get; set; }
        public string Description { get; set; }
        public EnumKAttributeDataType KAttributeDataType { get; set; }
        public bool RequiredValue { get; set; }
        public int Order { get; set; }
        public string Script { get; set; }
        public bool Disabled { get; set; }
        public Guid? NoteTypeId { get; set; }
    }
}
