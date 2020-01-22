using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KNote.Shared;

namespace KNote.DomainModel.Entities
{
    public class KLog : ModelBase
    {
        #region Constructor

        public KLog() : base() { }
        
        #endregion

        #region Property definitions

        private Guid _klogId;
        [Key]
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
        [Required(ErrorMessage = "KMSG: El Id de la entidad es requerido")]        
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
        [Required(ErrorMessage = "KMSG: El nombre de la entidad es requerido")]
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
        [Required(ErrorMessage = "KMSG: La gecha del registro es requerida")]
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
        [Required(ErrorMessage = "KMSG: El mensaje de registro es requerido")]
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
