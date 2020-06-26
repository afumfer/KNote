using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class TraceNoteDto : KntModelBase
    {
        public Guid TraceNoteId { get; set; }

        public Guid FromId { get; set; }

        public Guid ToId { get; set; }

        public int Order { get; set; }

        public double Weight { get; set; }
    }
}