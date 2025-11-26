using System.Collections.Generic;
using System.IO;

namespace KNote.Model.Dto;

public class NoteDto : NoteInfoDto
{
    private FolderInfoDto _folderDto;
    public FolderInfoDto FolderDto 
    {
        get {
            if (_folderDto == null)
                _folderDto = new FolderDto();
            return _folderDto; 
        }
        set
        {
            if (_folderDto != value)
            {
                _folderDto = value;
                OnPropertyChanged("FolderDto");
            }
        }
    }

    private NoteTypeDto _noteTypeDto;
    public NoteTypeDto NoteTypeDto
    {
        get
        {
            if (_noteTypeDto == null)
                _noteTypeDto = new NoteTypeDto();
            return _noteTypeDto;
        }
        set
        {
            if (_noteTypeDto != value)
            {
                _noteTypeDto = value;
                OnPropertyChanged("NoteTypeDto");}
        }
    }

    private List<NoteKAttributeDto> _kAttributesDto;
    public List<NoteKAttributeDto> KAttributesDto
    {
        get {
            if (_kAttributesDto == null)
                _kAttributesDto = new List<NoteKAttributeDto>();
            return _kAttributesDto; 
        }
        set
        {
            if (_kAttributesDto != value)
            {
                _kAttributesDto = value;
                OnPropertyChanged("KAttributesDto");
            }
        }
    }
}

