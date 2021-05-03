using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KNote.Model.Dto
{
    public class NotesFilterDto : NotesSearchDto
    {                     
        public Guid? FolderId { get; set; }
        public string Topic { get; set; }
        public Guid? NoteTypeId { get; set; }
        public string Tags { get; set; }        
        public string Description { get; set; }

        public List<AtrFilterDto> AttributesFilter { get; set; } = new List<AtrFilterDto>();

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = base.Validate(validationContext);

            //var results = new List<ValidationResult>();

            // ---
            // Capture the validations implemented with attributes.
            // ---

            //Validator.TryValidateProperty(this.EntityId,
            //   new ValidationContext(this, null, null) { MemberName = "EntityId" },
            //   results);

            // TODO: Añadir aquí el resto de validaciones vía atributos ....

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
    }

    public class AtrFilterDto : DtoModelBase
    {
        #region Properties 

        private Guid _atrId;
        [Required(ErrorMessage = KMSG)]
        public Guid AtrId
        {
            get { return _atrId; }
            set
            {
                if (_atrId != value)
                {
                    _atrId = value;
                    OnPropertyChanged("AtrId");
                }
            }
        }

        private string _atrName;
        public string AtrName
        {
            get { return _atrName; }
            set
            {
                if (_atrName != value)
                {
                    _atrName = value;
                    OnPropertyChanged("AtrName");
                }
            }
        }

        private string _value;
        [Required(ErrorMessage = KMSG)]
        public string Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged("Value");
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
            // TODO: (Esta sección se puede resolver por medio de reflexión).
            // ---

            Validator.TryValidateProperty(this.AtrId,
               new ValidationContext(this, null, null) { MemberName = "AtrId" },
               results);

            Validator.TryValidateProperty(this.Value,
               new ValidationContext(this, null, null) { MemberName = "Value" },
               results);

            // ---
            // Specific validations
            // ----


            return results;
        }

        #endregion 
    }
}
