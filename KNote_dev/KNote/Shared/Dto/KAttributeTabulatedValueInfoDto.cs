using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto
{
    public class KAttributeTabulatedValueInfoDto : KntModelBase
    {
        public Guid KAttributeTabulatedValueId { get; set; }
        public Guid KAttributeId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public int Order { get; set; }
    }
}
