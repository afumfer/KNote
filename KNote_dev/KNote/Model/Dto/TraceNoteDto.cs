using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class TraceNoteDto : DtoModelBase
    {
        //public Guid TraceNoteId { get; set; }

        //public Guid FromId { get; set; }

        //public Guid ToId { get; set; }

        //public int Order { get; set; }

        //public double Weight { get; set; }

        //public Guid? TraceNoteTypeId { get; set; }


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
