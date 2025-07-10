using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto;

public class FolderDto : FolderInfoDto
{
    public FolderDto ParentFolderDto { get; set; }  

    public List<FolderDto> ChildFolders { get; set; } = new List<FolderDto>();

    #region Utils for views

    public bool Selected { get; set; } = false;

    public bool Expanded { get; set; } = false;

    #endregion 
}
