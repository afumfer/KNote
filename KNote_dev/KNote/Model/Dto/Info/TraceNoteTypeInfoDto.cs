using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto.Info
{
    public class TraceNoteTypeInfoDto : KntModelBase
    {
        public Guid TraceNoteTypeId { get; set; }
        
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
