using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto.Info
{
    public class TraceNoteInfoDto : KntModelBase
    {
        public Guid TraceNoteId { get; set; }
        public Guid FromId { get; set; }
        public Guid ToId { get; set; }
        public int Order { get; set; }
        public double Weight { get; set; }
    }
}
