using System.Collections.Generic;
using System.IO;

namespace KNote.Model.Dto;

public class NoteDto : NoteInfoDto
{
    private FolderDto _folderDto;
    public FolderDto FolderDto 
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

    // TODO: Hack, this is temporary to solve the location of resources. We have to find another solution.
    public string ModelToViewDescription(RepositoryRef repositoryRef)
    {
        if (repositoryRef == null)
            return Description;
            
        string replaceString = "";
        if (!string.IsNullOrEmpty(repositoryRef?.ResourcesContainerCacheRootUrl))
        {
            replaceString = Path.Combine(repositoryRef?.ResourcesContainerCacheRootUrl, repositoryRef?.ResourcesContainer);
            replaceString = replaceString.Replace(@"\", @"/");
        }
        else
        {
            if (repositoryRef.ResourcesContainerCacheRootPath != null && repositoryRef?.ResourcesContainer != null)
            {
                replaceString = Path.Combine(repositoryRef?.ResourcesContainerCacheRootPath, repositoryRef?.ResourcesContainer);
                replaceString = replaceString.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
        }

        return Description?
            .Replace(repositoryRef.ResourcesContainer, replaceString);
    }

    // TODO: Hack, this is temporary to solve the location of resources. We have to find another solution.
    public string ViewToModelDescription(RepositoryRef repositoryRef, string contentView)
    {
        if (repositoryRef == null || string.IsNullOrEmpty(contentView))
            return contentView;

        string replaceString;
        if (!string.IsNullOrEmpty(repositoryRef?.ResourcesContainerCacheRootUrl))
        {
            replaceString = Path.Combine(repositoryRef?.ResourcesContainerCacheRootUrl, repositoryRef?.ResourcesContainer);
            replaceString = replaceString.Replace(@"\", @"/");
        }
        else
        {
            replaceString = Path.Combine(repositoryRef?.ResourcesContainerCacheRootPath, repositoryRef?.ResourcesContainer);
            replaceString = replaceString.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        return contentView
            .Replace(replaceString,
            repositoryRef.ResourcesContainer);
    }
}

