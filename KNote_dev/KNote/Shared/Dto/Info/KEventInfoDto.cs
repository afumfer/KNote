using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace KNote.Shared.Dto.Info
{
    public class KEventInfoDto : KntModelBase
    {
        public Guid KEventId { get; set; }
        // TODO: NoteScriptId debe ser null
        public Guid? NoteScriptId { get; set; }
        public Guid? EntityId { get; set; }
        public string EntityName { get; set; }
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
        public EnumEventType EventType { get; set; }
    }
}
