using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using KNote.Shared;

namespace KNote.DomainModel.Entities
{
    public class TraceNote : ModelBase
    {
        #region Constructor 

        public TraceNote() : base() { }
        
        #endregion

        #region Property definitions

        private Guid _traceNoteId;
        public Guid TraceNoteId
        {
            get { return _traceNoteId; }
            set
            {
                if (_traceNoteId != value)
                {
                    _traceNoteId = value;
                    OnPropertyChanged("TraceNoteId");
                }
            }
        }

        private Guid _fromId;        
        public Guid FromId
        {
            get { return _fromId; }
            set
            {
                if (_fromId != value)
                {
                    _fromId = value;
                    OnPropertyChanged("FromId");
                }
            }
        }

        private Guid _toId;
        public Guid ToId
        {
            get { return _toId; }
            set
            {
                if (_toId != value)
                {
                    _toId = value;
                    OnPropertyChanged("ToId");
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

        private double _weight;
        public double Weight
        {
            get { return _weight; }
            set
            {
                if (_weight != value)
                {
                    _weight = value;
                    OnPropertyChanged("Weight");
                }
            }
        }

        private Guid? _traceNoteTypeId;
        public Guid? TraceNoteTypeId
        {
            get { return _traceNoteTypeId; }
            set
            {
                if (_traceNoteTypeId != value)
                {
                    _traceNoteTypeId = value;
                    OnPropertyChanged("TraceNoteTypeId");
                }
            }
        }

        #region Virtual - navigation properties

        private Note _from;
        [InverseProperty("From")]
        [ForeignKey("FromId")]
        public virtual Note From
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

        private Note _to;
        [InverseProperty("To")]
        [ForeignKey("ToId")]
        public virtual Note To
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

        private TraceNoteType _traceNoteType;        
        public virtual TraceNoteType TraceNoteType
        {
            get { return _traceNoteType; }
            set
            {
                if (_traceNoteType != value)
                {
                    _traceNoteType = value;
                    OnPropertyChanged("TraceNoteType");
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

            //Validator.TryValidateProperty(this.Key,
            //   new ValidationContext(this, null, null) { MemberName = "Key" },
            //   results);

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
