using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class KMessageDto : DtoModelBase
    {
        #region Property definitions

        private Guid _kmessageId;        
        public Guid KMessageId
        {
            get { return _kmessageId; }
            set
            {
                if (_kmessageId != value)
                {
                    _kmessageId = value;
                    OnPropertyChanged("MessageId");
                }
            }
        }

        private Guid? _userId;
        public Guid? UserId
        {
            get { return _userId; }
            set
            {
                if (_userId != value)
                {
                    _userId = value;
                    OnPropertyChanged("UserId");
                }
            }
        }

        private Guid? _noteId;
        public Guid? NoteId
        {
            get { return _noteId; }
            set
            {
                if (_noteId != value)
                {
                    _noteId = value;
                    OnPropertyChanged("NoteId");
                }
            }
        }

        private EnumActionType _actionType;
        public EnumActionType ActionType
        {
            get { return _actionType; }
            set
            {
                if (_actionType != value)
                {
                    _actionType = value;
                    OnPropertyChanged("ActionType");
                }
            }
        }

        private EnumNotificationType _notificationType;
        public EnumNotificationType NotificationType
        {
            get { return _notificationType; }
            set
            {
                if (_notificationType != value)
                {
                    _notificationType = value;
                    OnPropertyChanged("NotificationType");
                }
            }
        }

        private EnumAlarmType _alarmType;
        public EnumAlarmType AlarmType
        {
            get { return _alarmType; }
            set
            {
                if (_alarmType != value)
                {
                    _alarmType = value;
                    OnPropertyChanged("AlarmType");
                }
            }
        }

        private bool _disabled;
        public bool Disabled
        {
            get { return _disabled; }
            set
            {
                if (_disabled != value)
                {
                    _disabled = value;
                    OnPropertyChanged("Disabled");
                }
            }
        }

        private string _content;
        public string Content
        {
            get { return _content; }
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged("Content");
                }
            }
        }

        private string _forward;
        public string Forward
        {
            get { return _forward; }
            set
            {
                if (_forward != value)
                {
                    _forward = value;
                    OnPropertyChanged("Forward");
                }
            }
        }

        private bool? _alarmOk;
        public bool? AlarmOk
        {
            get { return _alarmOk; }
            set
            {
                if (_alarmOk != value)
                {
                    _alarmOk = value;
                    OnPropertyChanged("AlarmOk");
                }
            }
        }

        private bool? _alarmActivated;
        public bool? AlarmActivated
        {
            get { return _alarmActivated; }
            set
            {
                if (_alarmActivated != value)
                {
                    _alarmActivated = value;
                    OnPropertyChanged("AlarmActivated");
                }
            }
        }

        private DateTime? _alarmDateTime;
        public DateTime? AlarmDateTime
        {
            get { return _alarmDateTime; }
            set
            {
                if (_alarmDateTime != value)
                {
                    _alarmDateTime = value;
                    OnPropertyChanged("AlarmDateTime");
                }
            }
        }

        private int? _alarmMinutes;
        public int? AlarmMinutes
        {
            get { return _alarmMinutes; }
            set
            {
                if (_alarmMinutes != value)
                {
                    _alarmMinutes = value;
                    OnPropertyChanged("AlarmMinutes");
                }
            }
        }

        public string UserFullName { get; set; }

        #endregion

        #region Validations

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // ---
            // Attributes validations 
            // ---            

            Validator.TryValidateProperty(this.Content,
               new ValidationContext(this, null, null) { MemberName = "Content" },
               results);

            //----
            // Specific validations 
            //----

            if (AlarmDateTime  == null)
            {
                results.Add(new ValidationResult
                 ("KMSG: The message or alarm date-time in mandatory."
                 , new[] { "AlarmDateTime" }));
            }

            // ---
            // Retornar List<ValidationResult>()
            // ---           

            return results;
        }

        #endregion
    }
}
