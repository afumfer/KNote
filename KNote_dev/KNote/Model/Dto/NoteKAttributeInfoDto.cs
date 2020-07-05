﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class NoteKAttributeInfoDto : DtoModelBase
    {
        //public Guid NoteKAttributeId { get; set; }
        //public Guid NoteId { get; set; }
        //public Guid KAttributeId { get; set; }
        //public string Value { get; set; }

        #region Property definitions

        private Guid _noteKAttributeId;        
        public Guid NoteKAttributeId
        {
            get { return _noteKAttributeId; }
            set
            {
                if (_noteKAttributeId != value)
                {
                    _noteKAttributeId = value;
                    OnPropertyChanged("NoteKAttributeId");
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

        private Guid _kattributeId;
        public Guid KAttributeId
        {
            get { return _kattributeId; }
            set
            {
                if (_kattributeId != value)
                {
                    _kattributeId = value;
                    OnPropertyChanged("KAttributeId");
                }
            }
        }

        private string _value;
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
            // Capturar las validaciones implementadas vía atributos.
            // ---            

            //Validator.TryValidateProperty(this.Name,
            //   new ValidationContext(this, null, null) { MemberName = "Name" },
            //   results);

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
}
