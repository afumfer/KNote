using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto;

public class KMessageDto : SmartModelDtoBase
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

    private string _comment;
    public string Comment
    {
        get { return _comment; }
        set
        {
            if (_comment != value)
            {
                _comment = value;
                OnPropertyChanged("Comment");
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

        Validator.TryValidateProperty(this.Comment,
           new ValidationContext(this, null, null) { MemberName = "Comment" },
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

        //
        if(AlarmType == EnumAlarmType.InMinutes && AlarmMinutes < 1)
        {
            results.Add(new ValidationResult
             ("KMSG: The minutes are mandatory in types of periodic alarms 'in minutes'."
             , new[] { "AlarmMinutes" }));
        }

        // ---
        // Return List<ValidationResult>()
        // ---           

        return results;
    }

    #endregion
}
