using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using KNote.Shared;

namespace KNote.DomainModel.Entities
{
    public class NoteType : ModelBase
    {
        #region Constructor

        public NoteType() : base() { }
        
        #endregion

        #region Property definitions

        private Guid _noteTypeId;
        [Key]
        public Guid NoteTypeId
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

        private string _key;
        [Required(ErrorMessage = "KMSG: La clave es requerida")]
        [MaxLength(32)]        
        public string Key
        {
            get { return _key; }
            set
            {
                if (_key != value)
                {
                    _key = value;
                    OnPropertyChanged("Key");
                }
            }
        }

        private string _name;
        [Required(ErrorMessage = "KMSG: El nombre del tipo es requerido")]
        [MaxLength(256)]        
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

        private Guid? _parentNoteTypeId;        
        public Guid? ParenNoteTypeId
        {
            get { return _parentNoteTypeId; }
            set
            {
                if (_parentNoteTypeId != value)
                {
                    _parentNoteTypeId = value;
                    OnPropertyChanged("ParenNoteTypeId");
                }
            }
        }

        #endregion

        #region Virtual - navigation properties

        private NoteType _parenNoteType;
        [ForeignKey("ParenNoteTypeId")]   
        public virtual NoteType ParenNoteType
        {
            get { return _parenNoteType; }
            set
            {
                if (_parenNoteType != value)
                {
                    _parenNoteType = value;
                    OnPropertyChanged("ParenNoteType");
                }
            }
        }

        #endregion 

        #region Validations

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // ---
            // Capturar las validaciones implementadas vía atributos.
            // ---

            Validator.TryValidateProperty(this.Key,
                new ValidationContext(this, null, null) { MemberName = "Key" },
                results);

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

