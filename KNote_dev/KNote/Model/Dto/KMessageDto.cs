using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class KMessageDto : KntModelBase
    {
        public Guid KMessageId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? NoteId { get; set; }
        public EnumActionType ActionType { get; set; }
        public EnumNotificationType NotificationType { get; set; }
        public EnumAlarmType AlarmType { get; set; }
        public bool Disabled { get; set; }
        public string Content { get; set; }
        public string Forward { get; set; }
        public bool? AlarmActivated { get; set; }
        public DateTime? AlarmDateTime { get; set; }
        public int? AlarmMinutes { get; set; }
        public bool? AlarmOk { get; set; }

        public UserDto UserDto { get; set; }
        public NoteDto NoteDto { get; set; }
    }
}
