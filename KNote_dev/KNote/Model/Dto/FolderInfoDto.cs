using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class FolderInfoDto : DtoModelBase
    {
        #region Properties 

        private Guid _folderId;
        public Guid FolderId
        {
            get { return _folderId; }
            set
            {
                if (_folderId != value)
                {
                    _folderId = value;
                    OnPropertyChanged("FolderId");
                }
            }
        }

        private int _folderNumber;
        public int FolderNumber
        {
            get { return _folderNumber; }
            set
            {
                if (_folderNumber != value)
                {
                    _folderNumber = value;
                    OnPropertyChanged("FolderNumber");
                }
            }
        }

        private DateTime _creationDateTime;        
        public DateTime CreationDateTime
        {
            get { return _creationDateTime; }
            set
            {
                if (_creationDateTime != value)
                {
                    _creationDateTime = value;
                    OnPropertyChanged("CreationDateTime");
                }
            }
        }

        private DateTime _modificationDateTime;        
        public DateTime ModificationDateTime
        {
            get { return _modificationDateTime; }
            set
            {
                if (_modificationDateTime != value)
                {
                    _modificationDateTime = value;
                    OnPropertyChanged("ModificationDateTime");
                }
            }
        }

        private string _name;
        [Required(ErrorMessage = "* Attribute {0} is required ")]
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

        private string _tags;
        [MaxLength(1024)]
        public string Tags
        {
            get { return _tags; }
            set
            {
                if (_tags != value)
                {
                    _tags = value;
                    OnPropertyChanged("Tags");
                }
            }
        }

        private string _pathFolder;
        [MaxLength(400)]
        public string PathFolder
        {
            get { return _pathFolder; }
            set
            {
                if (_pathFolder != value)
                {
                    _pathFolder = value;
                    OnPropertyChanged("PathFolder");
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

        private string _orderNotes;
        [MaxLength(256)]
        public string OrderNotes
        {
            get { return _orderNotes; }
            set
            {
                if (_orderNotes != value)
                {
                    _orderNotes = value;
                    OnPropertyChanged("OrderNotes");
                }
            }
        }

        private string _script;
        public string Script
        {
            get { return _script; }
            set
            {
                if (_script != value)
                {
                    _script = value;
                    OnPropertyChanged("Script");
                }
            }
        }

        private Guid? _parentId;
        public Guid? ParentId
        {
            get { return _parentId; }
            set
            {
                if (_parentId != value)
                {
                    _parentId = value;
                    OnPropertyChanged("ParentId");
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
            // TODO: (Esta sección se puede resolver por medio de reflexión).
            // ---

            Validator.TryValidateProperty(this.Name,
               new ValidationContext(this, null, null) { MemberName = "Name" },
               results);

            Validator.TryValidateProperty(this.PathFolder,
               new ValidationContext(this, null, null) { MemberName = "PathFolder" },
               results);

            // ---
            // Validaciones específicas
            // ----


            return results;
        }

        #endregion 
    }
}
