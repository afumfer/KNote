using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto.Info
{
    public class KAttributeTabulatedValueInfoDto : KntModelBase
    {
        public Guid KAttributeTabulatedValueId { get; set; }
        public Guid KAttributeId { get; set; }        
        public string Value { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
    }
}
