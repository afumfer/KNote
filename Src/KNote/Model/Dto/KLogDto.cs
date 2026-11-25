using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto;

public class KLogDto : SmartModelDtoBase
{        
    #region Property definitions

    private Guid _klogId;        
    public Guid KLogId
    {
        get { return _klogId; }
        set
        {
            if (_klogId != value)
            {
                _klogId = value;
                OnPropertyChanged("KLogId");
            }
        }
    }

    private Guid _entityId;
    [Required(ErrorMessage = KMSG)]
    public Guid EntityId
    {
        get { return _entityId; }
        set
        {
            if (_entityId != value)
            {
                _entityId = value;
                OnPropertyChanged("EntityId");
            }
        }
    }

    private string _entityName;
    [Required(ErrorMessage = KMSG)]
    [MaxLength(64)]
    public string EntityName
    {
        get { return _entityName; }
        set
        {
            if (_entityName != value)
            {
                _entityName = value;
                OnPropertyChanged("EntityName");
            }
        }
    }

    private DateTime _registryDateTime;
    [Required(ErrorMessage = KMSG)]
    public DateTime RegistryDateTime
    {
        get { return _registryDateTime; }
        set
        {
            if (_registryDateTime != value)
            {
                _registryDateTime = value;
                OnPropertyChanged("RegistryDateTime");
            }
        }
    }

    private string _registryMessage;
    [Required(ErrorMessage = KMSG)]
    public string RegistryMessage
    {
        get { return _registryMessage; }
        set
        {
            if (_registryMessage != value)
            {
                _registryMessage = value;
                OnPropertyChanged("RegistryMessage");
            }
        }
    }

    private string _actionType;
    [MaxLength(64)]
    public string ActionType
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

    // The history of the notes (schema revision 11). RegistryDateTime is when the session of changes started and
    // LastDateTime its last change (H5). PreviousContent, the version, is not here: the lists never read it
    // (IKntHistoryRepository.GetPreviousContentAsync); PreviousContentSize says whether the row has one.

    public const int MaxUserNameLength = 256;
    public const int MaxOriginLength = 32;
    public const int MaxEntityTitleLength = 512;

    private DateTime _lastDateTime;
    public DateTime LastDateTime
    {
        get { return _lastDateTime; }
        set
        {
            if (_lastDateTime != value)
            {
                _lastDateTime = value;
                OnPropertyChanged("LastDateTime");
            }
        }
    }

    private string _userName;
    [MaxLength(MaxUserNameLength)]
    public string UserName
    {
        get { return _userName; }
        set
        {
            if (_userName != value)
            {
                _userName = value;
                OnPropertyChanged("UserName");
            }
        }
    }

    // EnumHistoryOrigin, by its name.
    private string _origin;
    [MaxLength(MaxOriginLength)]
    public string Origin
    {
        get { return _origin; }
        set
        {
            if (_origin != value)
            {
                _origin = value;
                OnPropertyChanged("Origin");
            }
        }
    }

    private EnumHistoryFields _fields;
    public EnumHistoryFields Fields
    {
        get { return _fields; }
        set
        {
            if (_fields != value)
            {
                _fields = value;
                OnPropertyChanged("Fields");
            }
        }
    }

    // NoteChangeDetailsDto as JSON: what changed.
    private string _changeDetails;
    public string ChangeDetails
    {
        get { return _changeDetails; }
        set
        {
            if (_changeDetails != value)
            {
                _changeDetails = value;
                OnPropertyChanged("ChangeDetails");
            }
        }
    }

    // What the entity was called (#340 Topic), also once deleted.
    private string _entityTitle;
    [MaxLength(MaxEntityTitleLength)]
    public string EntityTitle
    {
        get { return _entityTitle; }
        set
        {
            if (_entityTitle != value)
            {
                _entityTitle = value;
                OnPropertyChanged("EntityTitle");
            }
        }
    }

    // The size of PreviousContent in bytes (UTF-8), or null if the row has no version.
    private int? _previousContentSize;
    public int? PreviousContentSize
    {
        get { return _previousContentSize; }
        set
        {
            if (_previousContentSize != value)
            {
                _previousContentSize = value;
                OnPropertyChanged("PreviousContentSize");
            }
        }
    }

    public bool HasPreviousContent => PreviousContentSize != null;

    #endregion

    #region Validations

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        // ---
        // Capture the validations implemented with attributes.
        // ---

        Validator.TryValidateProperty(this.EntityId,
           new ValidationContext(this, null, null) { MemberName = "EntityId" },
           results);

        // TODO: Añadir aquí el resto de validaciones vía atributos ....

        //----
        // Specific validations
        //----

        // ---- Ejemplo
        //if (ModificationDateTime < CreationDateTime)
        //{
        //    results.Add(new ValidationResult
        //     ("The modification date cannot be greater than the creation date "
        //     , new[] { "ModificationDateTime", "CreationDateTime" }));
        //}

        // ---
        // Return List<ValidationResult>()
        // ---           

        return results;
    }

    #endregion
}