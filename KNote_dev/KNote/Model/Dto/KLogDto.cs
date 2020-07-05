using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class KLogDto : DtoModelBase
    {
        #region Property definitions

        private Guid _klogId;        
        public Guid KLogId
        {
            get { return _klogId; }
            set
            {
                if (_klogId != value)
                {
                    _klogId = value;
                    OnPropertyChanged("KLogId");
                }
            }
        }

        private Guid _entityId;
        [Required(ErrorMessage = "* Attribute {0} is required ")]
        public Guid EntityId
        {
            get { return _entityId; }
            set
            {
                if (_entityId != value)
                {
                    _entityId = value;
                    OnPropertyChanged("EntityId ");
                }
            }
        }

        private string _entityName;
        [Required(ErrorMessage = "* Attribute {0} is required ")]
        [MaxLength(64)]
        public string EntityName
        {
            get { return _entityName; }
            set
            {
                if (_entityName != value)
                {
                    _entityName = value;
                    OnPropertyChanged("EntityName ");
                }
            }
        }

        private DateTime _registryDateTime;
        [Required(ErrorMessage = "* Attribute {0} is required ")]
        public DateTime RegistryDateTime
        {
            get { return _registryDateTime; }
            set
            {
                if (_registryDateTime != value)
                {
                    _registryDateTime = value;
                    OnPropertyChanged("RegistryDateTime ");
                }
            }
        }

        private string _registryMessage;
        [Required(ErrorMessage = "* Attribute {0} is required ")]
        public string RegistryMessage
        {
            get { return _registryMessage; }
            set
            {
                if (_registryMessage != value)
                {
                    _registryMessage = value;
                    OnPropertyChanged("RegistryMessage ");
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

            Validator.TryValidateProperty(this.EntityId,
               new ValidationContext(this, null, null) { MemberName = "EntityId" },
               results);

            // TODO: Añadir aquí el resto de validaciones vía atributos ....

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