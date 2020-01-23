﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

using KNote.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace KNote.DomainModel.Entities
{
    public class Note : ModelBase
    {
        #region Constructor

        public Note() : base() { }
        
        #endregion

        #region Property definitions

        private Guid _noteId;
        [Key]
        public Guid NoteId
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

        private int _noteNumber;        
        public int  NoteNumber
        {
            get { return _noteNumber; }
            set
            {
                if (_noteNumber != value)
                {
                    _noteNumber = value;
                    OnPropertyChanged("NoteNumber");
                }
            }
        }

        private string _topic;
        [Required (ErrorMessage= "KMSG: El asunto es requerido")]
        [MaxLength(1024)]        
        public string Topic
        {
            get { return _topic; }
            set
            {
                if (_topic != value)
                {
                    _topic = value;
                    OnPropertyChanged("Topic");
                }
            }
        }

        private DateTime _creationDateTime;
        [Required]
        public DateTime CreationDateTime
        {
            get { return _creationDateTime; }
            set
            {
                if (_creationDateTime != value)
                {
                    _creationDateTime = value;
                    OnPropertyChanged("CreationDateTime");
                }
            }
        }

        private DateTime _modificationDateTime;
        [Required]
        public DateTime ModificationDateTime
        {
            get { return _modificationDateTime; }
            set
            {
                if (_modificationDateTime != value)
                {
                    _modificationDateTime = value;
                    OnPropertyChanged("ModificationDateTime");
                }
            }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        private string _contentType;
        [MaxLength(1024)]
        public string ContentType
        {
            get { return _contentType; }
            set
            {
                if (_contentType != value)
                {
                    _contentType = value;
                    OnPropertyChanged("ContentType");
                }
            }
        }

        private string _script;
        public string Script
        {
            get { return _script; }
            set
            {
                if (_script != value)
                {
                    _script = value;
                    OnPropertyChanged("Script");
                }
            }
        }

        private string _internalTags;
        [MaxLength(256)]        
        public string InternalTags
        {
            get { return _internalTags; }
            set
            {
                if (_internalTags != value)
                {
                    _internalTags = value;
                    OnPropertyChanged("InternalTags");
                }
            }
        }

        private string _tags;
        [MaxLength(1024)]
        public string Tags
        {
            get { return _tags; }
            set
            {
                if (_tags != value)
                {
                    _tags = value;
                    OnPropertyChanged("Tags");
                }
            }
        }

        private int _priority;
        public int Priority
        {
            get { return _priority; }
            set
            {
                if (_priority != value)
                {
                    _priority = value;
                    OnPropertyChanged("Priority");
                }
            }
        }
        
        private Guid _folderId;
        public Guid FolderId
        {
            get { return _folderId; }
            set
            {
                if (_folderId != value)
                {
                    _folderId = value;
                    OnPropertyChanged("FolderId");
                }
            }
        }

        private Guid? _noteTypeId;
        public Guid? NoteTypeId
        {
            get { return _noteTypeId; }
            set
            {
                if (_noteTypeId != value)
                {
                    _noteTypeId = value;
                    OnPropertyChanged("NoteTypeId");
                }
            }
        }

        #region Virtual - navigation properties

        private Folder _folder;        
        public virtual Folder Folder
        {
            get { return _folder; }
            set
            {
                if (_folder != value)
                {
                    _folder = value;
                    OnPropertyChanged("Folder");
                }
            }
        }
         
        private NoteType _noteType;        
        public virtual NoteType NoteType
        {
            get { return _noteType; }
            set
            {
                if (_noteType != value)
                {
                    _noteType = value;
                    OnPropertyChanged("NoteType");
                }
            }
        }

        private List<NoteKAttribute> _kAttributes;
        public virtual List<NoteKAttribute> KAttributes
        {
            get { return _kAttributes; }
            set
            {
                if (_kAttributes != value)
                {
                    _kAttributes = value;
                    OnPropertyChanged("KAttributes");
                }
            }
        }

        private List<Resource> _resources;
        public virtual List<Resource> Resources
        {
            get { return _resources; }
            set
            {
                if (_resources != value)
                {
                    _resources = value;
                    OnPropertyChanged("Resources");
                }
            }
        }

        private List<NoteTask> _tasks;
        public virtual List<NoteTask> NoteTasks
        {
            get { return _tasks; }
            set
            {
                if (_tasks != value)
                {
                    _tasks = value;
                    OnPropertyChanged("Tasks");
                }
            }
        }

        private List<Window> _windows;
        public virtual List<Window> Windows
        {
            get { return _windows; }
            set
            {
                if (_windows != value)
                {
                    _windows = value;
                    OnPropertyChanged("Windows");
                }
            }
        }

        private List<TraceNote> _from;
        public virtual List<TraceNote> From
        {
            get { return _from; }
            set
            {
                if (_from != value)
                {
                    _from = value;
                    OnPropertyChanged("From");
                }
            }
        }

        private List<TraceNote> _to;
        public virtual List<TraceNote> To
        {
            get { return _to; }
            set
            {
                if (_to != value)
                {
                    _to = value;
                    OnPropertyChanged("To");
                }
            }
        }

        private List<KMessage> _kMessages;
        public virtual List<KMessage> KMessages
        {
            get { return _kMessages; }
            set
            {
                if (_kMessages != value)
                {
                    _kMessages = value;
                    OnPropertyChanged("KMessages");
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

            Validator.TryValidateProperty(this.Topic,
               new ValidationContext(this, null, null) { MemberName = "Topic" },
               results);

            Validator.TryValidateProperty(this.InternalTags,
               new ValidationContext(this, null, null) { MemberName = "InternalTags" },
               results);

            Validator.TryValidateProperty(this.Tags,
               new ValidationContext(this, null, null) { MemberName = "Tags" },
               results);

            //----
            // Validaciones específicas
            //----

            if (ModificationDateTime < CreationDateTime)
            {
                results.Add(new ValidationResult
                 ("KMSG: La fecha de modificación no puede ser mayor que la fecha de creación"
                 , new[] { "ModificationDateTime", "CreationDateTime" }));
            }

            // TODO: Validar NoteNumber

            // TODO: .....

            // ---
            // Retornar List<ValidationResult>()
            // ---           

            return results;
        }

        #endregion
    }
}