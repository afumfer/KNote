using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto;

public class ResourceInfoDto : SmartModelDtoBase
{
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

    // KNote Atlas, stage 4 (D12, ResourceDescriptionRules): the description is locked (a person's: the AI doesn't change
    // it), and the model that wrote it (null: not the AI). The save command decides both: the model never comes from
    // outside.
    private bool _descriptionByUser;
    public bool DescriptionByUser
    {
        get { return _descriptionByUser; }
        set
        {
            if (_descriptionByUser != value)
            {
                _descriptionByUser = value;
                OnPropertyChanged("DescriptionByUser");
            }
        }
    }

    private string _descriptionModel;
    public string DescriptionModel
    {
        get { return _descriptionModel; }
        set
        {
            if (_descriptionModel != value)
            {
                _descriptionModel = value;
                OnPropertyChanged("DescriptionModel");
            }
        }
    }

    /// <summary>
    /// The description the resource was read with (the Service sets it on every read, the editors keep it; not a column,
    /// and it doesn't make the resource dirty). Saving compares the text with it: if it is the same, the saved one stays,
    /// also if the AI wrote it meanwhile. Null: a resource not read from KNote (code that builds it).
    /// </summary>
    public string LoadedDescription { get; set; }

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
                var typeStringLen = value.Length > 64 ? 64 : value.Length;
                _fileType = value.Substring(0, typeStringLen);  // TODO: hack, the database attribute is 64                
                OnPropertyChanged("FileType");
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
    public virtual string ContentBase64
    {
        get
        {
            return _contentBase64;
        }
        set
        {
            if (_contentBase64 != value)
            {
                _contentBase64 = value;                    
                OnPropertyChanged("ContentBase64");
            }
        }
    }

    public string NameOut
    {
        get { return FileNameOf(Name); }
        set { }
    }

    /// <summary>
    /// The file name of a resource without the note's guid prefix ("{guid}_{name}"; the file store may have written the
    /// guid's dashes as underscores).
    /// </summary>
    public static string FileNameOf(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;
        if (name.Length > 37 && name[36] == '_' && Guid.TryParse(name.Substring(0, 36).Replace('_', '-'), out _))
            return name.Substring(37);
        return name;
    }

    /// <summary>The short title: the first sentence of its description or, without one of its own, the file name.</summary>
    public string GetTitle(int maxLength = ResourceDescriptionRules.TitleLength) =>
        ResourceDescriptionRules.Title(Description, Name, maxLength);

    /// <summary>Who wrote the description, to show under it.</summary>
    public string GetDescriptionAuthor() =>
        ResourceDescriptionRules.Author(Description, DescriptionByUser, DescriptionModel, Name);

    private string _fullUrl;
    public string FullUrl
    {
        get { return _fullUrl; }
        set
        {
            if (_fullUrl != value)
            {
                _fullUrl = value;
                OnPropertyChanged("FullUrl");
            }
        }
    }

    private string _relativeUrl;
    public string RelativeUrl
    {
        get { return _relativeUrl; }
        set
        {
            if (_relativeUrl != value)
            {
                _relativeUrl = value;
                OnPropertyChanged("RelativeUrl");
            }
        }
    }

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

        // A new resource comes with its file. An existing one may come without it (only its data changes).
        if (_contentBase64 == null && ResourceId == Guid.Empty)
        {
            results.Add(new ValidationResult
             ("If the content is in the database, you must enter its content in base 64  "
             , new[] { "ContentBase64" }));
        }

        return results;
    }
}
