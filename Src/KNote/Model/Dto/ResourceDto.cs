using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class ResourceDto : SmartModelDtoBase
    {        
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
        [Required(ErrorMessage = KMSG)]
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

        private bool _contentInDB = true;
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

        private byte[] _contentArrayBytes;
        public byte[] ContentArrayBytes
        {
            get 
            {
                if(_contentArrayBytes == null)
                {
                    if (_contentBase64 != null)
                        _contentArrayBytes = Convert.FromBase64String(_contentBase64); 
                    else
                        _contentArrayBytes = null;
                }
                return _contentArrayBytes; 
            }
            set
            {
                if (_contentArrayBytes != value)
                {
                    _contentArrayBytes = value;
                    _contentBase64 = null;
                    OnPropertyChanged("ContentArrayBytes");
                }
            }
        }

        private string _contentBase64;        
        public string ContentBase64 {            
            get
            {
                if (_contentBase64 == null)
                {
                    if (_contentArrayBytes != null)
                        _contentBase64 = Convert.ToBase64String(_contentArrayBytes);
                    else
                        _contentBase64 = null;
                }
                return _contentBase64;
            }
            set
            {
                if(_contentBase64 != value)
                {
                    _contentBase64 = value;
                    _contentArrayBytes = null;
                    OnPropertyChanged("ContentBase64");
                }
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
                    // Drop the guid prefix from filename 
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
            // Capture the validations implemented with attributes.
            // ---

            Validator.TryValidateProperty(this.Name,
               new ValidationContext(this, null, null) { MemberName = "Name" },
               results);

            Validator.TryValidateProperty(this.Container,
               new ValidationContext(this, null, null) { MemberName = "Container" },
               results);


            //----
            // Specific validations
            //----

            if (ContentInDB == true && ContentArrayBytes == null)
            {
                results.Add(new ValidationResult
                 ("KMSG: If the content is in the database, you must enter its content in base 64  "
                 , new[] { "ContentInDB", "ContentBase64" }));
            }

            return results;
        }

        #endregion
    }
}