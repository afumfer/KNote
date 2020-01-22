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

        private string _path;
        [Required(ErrorMessage = "KMSG: El nombre del recurso es requerido")]
        [MaxLength(1024)]        
        public string Path
        {
            get { return _path; }
            set
            {
                if (_path != value)
                {
                    _path = value;
                    OnPropertyChanged("Path");
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

        private string _fileMimeType;
        [MaxLength(64)]
        public string FileMimeType
        {
            get { return _fileMimeType; }
            set
            {
                if (_fileMimeType != value)
                {
                    _fileMimeType = value;
                    OnPropertyChanged("FileMimeType");
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

        private byte[] _contentDB;
        public byte[] ContentDB
        {
            get { return _contentDB; }
            set
            {
                if (_contentDB != value)
                {
                    _contentDB = value;
                    OnPropertyChanged("ContentDB");
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

            Validator.TryValidateProperty(this.Path,
               new ValidationContext(this, null, null) { MemberName = "Path" },
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

