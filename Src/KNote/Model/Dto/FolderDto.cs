﻿using System;
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
    // TODO: Icon es provisional, 
    public string Icon { get; set; } = "fa-folder";

    public string ShortName
    {
        get
        {
            if (Name?.Length > 45)
                return Name.Substring(0, 42) + "...";
            else
                return Name;
        }
    }

    public bool Selected { get; set; } = false;

    public bool Expanded { get; set; } = false;

    public void Toggle()
    {
        Expanded = !Expanded;
    }

    public string getIcon()
    {
        if (Expanded)
        {
            //return "-";
            return "oi oi-caret-bottom";
            //return "oi oi-chevron-bottom";
        }

        //return "+";
        return "oi oi-caret-right";
        //return "oi oi-chevron-right";
    }

    public string getColor()
    {
        if (Selected)
            return "text-light bg-dark";
        return "text-dark";
    }

    #endregion 
}
