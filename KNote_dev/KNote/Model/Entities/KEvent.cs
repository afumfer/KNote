using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KNote.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace KNote.Model.Entities
{
    public class KEvent : ModelBase
    {
        #region Constructor

        public KEvent() : base() { }
        
        #endregion

        #region Property definitions

        private Guid _keventId;
        [Key]
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

        #region Virtual - navigation properties

        private Note _noteScript;        
        [ForeignKey("NoteScriptId")]
        public virtual Note NoteScript
        {
            get { return _noteScript; }
            set
            {
                if (_noteScript != value)
                {
                    _noteScript = value;
                    OnPropertyChanged("NoteScript");
                }
            }
        }

        #endregion 

        #endregion

        #region Validations

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // ---
            // Capturar las validaciones implementadas vía atributos.
            // ---            

            Validator.TryValidateProperty(this.EntityName,
               new ValidationContext(this, null, null) { MemberName = "EntityName" },
               results);

            Validator.TryValidateProperty(this.PropertyName,
               new ValidationContext(this, null, null) { MemberName = "PropertyName" },
               results);

            //----
            // Validaciones específicas
            //----

            //if (ModificationDateTime < CreationDateTime)
            //{
            //    results.Add(new ValidationResult
            //     ("KMSG: La fecha de modificación no puede ser mayor que la fecha de creación"
            //     , new[] { "ModificationDateTime", "CreationDateTime" }));
            //}

            // ---
            // Retornar List<ValidationResult>()
            // ---           

            return results;
        }

        #endregion
    }

    public enum EnumEventType
    {        
        OnCreateActionDefault,
        OnSaveActionDefault,
        OnDeleteActionDefault,
        OnPropertyGetValueActionDefault,
        OnPropertyChangeActionDefault,
        OnCreateScriptExec,
        OnSaveScriptExec,
        OnDeleteScriptExec,
        OnPropertyGetValueScriptExec,
        OnPropertyChangeScriptExec
    }
}
