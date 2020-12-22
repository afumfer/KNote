using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class ResourceDto : DtoModelBase
    {
        private const string KMSG = "Attribute {0} is required. ";

        #region Property definitions

        private Guid _resourceId;        
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
        [Required(ErrorMessage =KMSG)]
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

        private string _contentBase64;
        public string ContentBase64 {
            get
            {
                if (_contentBase64 == null)
                    _contentBase64 = Convert.ToBase64String(_contentArrayBytes);
                return _contentBase64;
            }
            set
            {
                _contentBase64 = value;
            } 
        }

        public string NameOut
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return Name;
                else
                {
                    // Ehurística para descartar el prefijo (guid) del nombre del fichero
                    var i = Name.IndexOf("_") + 1;
                    if (i == 37)
                        return Name.Substring(i, Name.Length - i);
                    else
                        return Name;
                }
            }
            set { }
        }

        public string FullUrl { get; set; }

        public string RelativeUrl { get; set; }

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