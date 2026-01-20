using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KNote.Model.Dto;

public class NoteInfoDto : NoteMinimalDto
{        
    #region Property definitions

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

    private string _contentType;
    [MaxLength(1024)]
    public string ContentType
    {
        get 
        {
            if (_contentType == null)
                _contentType = "markdown";
            return _contentType; 
        }
        set
        {
            if (_contentType != value)
            {                
                _contentType = value;
                OnPropertyChanged("ContentType");
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

    private Guid? _noteTypeId;
    public Guid? NoteTypeId
    {
        get { return _noteTypeId; }
        set
        {
            if (_noteTypeId != value)
            {
                _noteTypeId = value;
                OnPropertyChanged("NoteTypeId");
            }
        }
    }

    #endregion 

    #region Validations

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
       return base.Validate(validationContext);
    }

    #endregion

    #region Util ContentTypeExt

    public ContentTypeExt GetContentTypeExt()
    {        
        if (ContentTypeExtJsonHelper.TryDeserialize(_contentType, out var tryDes))
            return tryDes;
        return new ContentTypeExt(_contentType, "", false);        
    }

    public void SetContentTypeExt(ContentTypeExt value)
    {     
        _contentType = ContentTypeExtJsonHelper.Serialize(value);
        OnPropertyChanged("ContentType");     
    }

    #endregion

}

public record ContentTypeExt(string Desciption, string Script, bool DescriptionBlocked);

public static class ContentTypeExtJsonHelper
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static string Serialize(ContentTypeExt obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        return JsonSerializer.Serialize(obj, Options);
    }
    
    public static ContentTypeExt Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) throw new ArgumentNullException(nameof(json));
        return JsonSerializer.Deserialize<ContentTypeExt>(json, Options);
    }

    public static bool TryDeserialize(string json, out ContentTypeExt result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(json)) return false;
        try
        {
            result = JsonSerializer.Deserialize<ContentTypeExt>(json, Options);
            return result != null;
        }
        catch
        {
            return false;
        }
    }
}