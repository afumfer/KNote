using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KNote.Model.Dto
{
    public class NoteTypeDto : DtoModelBase
    {
        #region Property definitions

        private Guid _noteTypeId;        
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

        private string _name;
        [Required(ErrorMessage = "* Attribute {0} is required ")]
        [MaxLength(256)]
        public string Name
        {
            get {
                if (_name == null)
                    _name = "(Enter new type note name)";
                return _name; 
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
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

        #region Validations

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // ---
            // Capture the validations implemented with attributes.
            // ---

            Validator.TryValidateProperty(this.Name,
               new ValidationContext(this, null, null) { MemberName = "Name" },
               results);

            //----
            // Specific validations
            //----

            // ---- Ejemplo
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
