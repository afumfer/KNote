using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using KNote.Shared;

namespace KNote.DomainModel.Entities
{
    public class Resource : ModelBase
    {
        #region Constructor

        public Resource() : base() { }
        
        #endregion

        #region Property definitions

        private Guid _resourceId;
        [Key]
        public Guid ResourceId
        {
            get { return _resourceId; }
            set
            {
                if (_resourceId != value)
                {
                    _resourceId = value;
                    OnPropertyChanged("ResourceId");
                }
            }
        }

        private string _name;
        [Required(ErrorMessage = "KMSG: El nombre del recurso es requerido")]
        [MaxLength(1024)]        
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        private string _container;
        public string Container
        {
            get { return _container; }
            set
            {
                if (_container != value)
                {
                    _container = value;
                    OnPropertyChanged("Container");
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

        private int _order;
        public int Order
        {
            get { return _order; }
            set
            {
                if (_order != value)
                {
                    _order = value;
                    OnPropertyChanged("Order");
                }
            }
        }

        private string _fileType;
        [MaxLength(64)]
        public string FileType
        {
            get { return _fileType; }
            set
            {
                if (_fileType != value)
                {
                    _fileType = value;
                    OnPropertyChanged("FileType");
                }
            }
        }

        private bool _contentInDB;
        public bool ContentInDB
        {
            get { return _contentInDB; }
            set
            {
                if (_contentInDB != value)
                {
                    _contentInDB = value;
                    OnPropertyChanged("ContentInDB");
                }
            }
        }

        private byte[] _contentArrayBytes;
        public byte[] ContentArrayBytes
        {
            get { return _contentArrayBytes; }
            set
            {
                if (_contentArrayBytes != value)
                {
                    _contentArrayBytes = value;
                    OnPropertyChanged("ContentArrayBytes");
                }
            }
        }

        private Guid _noteId;
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

        #region Virtual - navigation properties

        private Note _note;        
        public virtual Note Note
        {
            get { return _note; }
            set
            {
                if (_note != value)
                {
                    _note = value;
                    OnPropertyChanged("Note");
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

            Validator.TryValidateProperty(this.Name,
               new ValidationContext(this, null, null) { MemberName = "Name" },
               results);

            //----
            // Validaciones específicas
            //----

            // ---- Ejemplo
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
}

