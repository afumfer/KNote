using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

using KNote.Shared;

namespace KNote.Shared.Entities
{
    public class Folder : ModelBase
    {
        #region Constructor

        public Folder() : base() {}
        
        #endregion

        #region Property definitions

        private Guid _folderId;
        [Key]
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
        [Required]
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
        [Required]
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
        [Required(ErrorMessage = "KMSG: Name required")]
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

        #region Virtual - navigation properties

        private Folder _parentFolder;
        [ForeignKey("ParentId")]   // Este atributo no va bien al desplazarlo a fluent api, se debe quedar aquí. 
        public virtual Folder ParentFolder
        {
            get { return _parentFolder; }
            set
            {
                if (_parentFolder != value)
                {
                    _parentFolder = value;
                    OnPropertyChanged("ParentFolder");
                }
            }
        }

        private List<Folder> _childsFolders;
        public virtual List<Folder> ChildsFolders
        {
            get { return _childsFolders; }
            set
            {
                if (_childsFolders != value)
                {
                    _childsFolders = value;
                    OnPropertyChanged("ChildsFolders");
                }
            }
        }

        private List<Note> _notes;
        public virtual List<Note> Notes
        {
            get { return _notes; }
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged("Notes");
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
           
            if (ModificationDateTime < CreationDateTime)
            {
                results.Add(new ValidationResult
                 ("KMSG: La fecha de modificación no puede ser mayor que la fecha de creación"
                 , new[] { "ModificationDateTime", "CreationDateTime" }));
            }

            // TODO: FolderNumber debe ser único

            // ---
            // Retornar List<ValidationResult>()
            // ---

            return results;
        }

        #region Otra versión (ligera): Versión yield de Validate 
        //public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    // TODO: Capturar las validaciones implementadas vía atributos.
        //    var results = new List<ValidationResult>();
        //    Validator.TryValidateProperty(this.Name,
        //       new ValidationContext(this, null, null) { MemberName = "Name" },
        //       results);

        //    // TODO: Probar esto 
        //    Validator.TryValidateProperty(this.OrderNodes,
        //       new ValidationContext(this, null, null) { MemberName = "OrderNodes" },
        //       results);

        //    foreach (ValidationResult v in results)
        //        yield return v;
        //    //--------

        //    // Validaciones específicas
        //    if (ModificationDateTime < CreationDateTime)
        //    {
        //        yield return new ValidationResult
        //         ("La fecha de modificación no puede ser mayor que la fecha de creación"
        //         , new[] { "ModificationDateTime", "CreationDateTime" });
        //    }
        //}
        #endregion


        #endregion
    }
}
