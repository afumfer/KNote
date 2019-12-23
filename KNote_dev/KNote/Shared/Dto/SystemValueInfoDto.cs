using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto
{
    public class SystemValueInfoDto : KntModelBase 
    {
        public Guid SystemValueId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
