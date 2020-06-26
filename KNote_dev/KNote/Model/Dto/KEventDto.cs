using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class KEventDto : KntModelBase
    {
        public Guid KEventId { get; set; }

        // TODO: NoteScriptId debe ser null
        public Guid? NoteScriptId { get; set; }

        public Guid? EntityId { get; set; }

        [MaxLength(64)]
        public string EntityName { get; set; }

        [MaxLength(64)]
        public string PropertyName { get; set; }

        public string PropertyValue { get; set; }

        public EnumEventType EventType { get; set; }

        public NoteDto NoteScriptDto { get; set; }
    }
}
