using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class KEventDto : DtoModelBase
    {
        #region Property definitions

        private Guid _keventId;        
        public Guid KEventId
        {
            get { return _keventId; }
            set
            {
                if (_keventId != value)
                {
                    _keventId = value;
                    OnPropertyChanged("KEventId");
                }
            }
        }

        private Guid? _noteScriptId;
        public Guid? NoteScriptId
        {
            get { return _noteScriptId; }
            set
            {
                if (_noteScriptId != value)
                {
                    _noteScriptId = value;
                    OnPropertyChanged("NoteScriptId");
                }
            }
        }

        private Guid? _entityId;
        public Guid? EntityId
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

        private string _propertyName;
        [MaxLength(64)]
        public string PropertyName
        {
            get { return _propertyName; }
            set
            {
                if (_propertyName != value)
                {
                    _propertyName = value;
                    OnPropertyChanged("PropertyName");
                }
            }
        }

        private string _propertyValue;
        public string PropertyValue
        {
            get { return _propertyValue; }
            set
            {
                if (_propertyValue != value)
                {
                    _propertyValue = value;
                    OnPropertyChanged("PropertyValue");
                }
            }
        }

        private EnumEventType _eventType;
        public EnumEventType EventType
        {
            get { return _eventType; }
            set
            {
                if (_eventType != value)
                {
                    _eventType = value;
                    OnPropertyChanged("EventType");
                }
            }
        }

        #endregion 

        #region Validations

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // ---
            // Capture the validations implemented with attributes.
            // ---            

            Validator.TryValidateProperty(this.EntityName,
               new ValidationContext(this, null, null) { MemberName = "EntityName" },
               results);

            Validator.TryValidateProperty(this.PropertyName,
               new ValidationContext(this, null, null) { MemberName = "PropertyName" },
               results);

            //----
            // Specific validations
            //----

            //if (ModificationDateTime < CreationDateTime)
            //{
            //    results.Add(new ValidationResult
            //     ("KMSG: The modification date cannot be greater than the creation date "
            //     , new[] { "ModificationDateTime", "CreationDateTime" }));
            //}

            // ---
            // Return List<ValidationResult>()
            // ---           

            return results;
        }

        #endregion
    }
}
