using System;
using System.Collections.Generic;
using System.Text;

using KNote.Model.Dto;
using KNote.Service;

namespace KNote.ClientWin.Core
{
    public class FolderWithServiceRef
    {
        public FolderInfoDto FolderInfo { get; set; }
        public ServiceRef ServiceRef { get; set; }
    }
}
