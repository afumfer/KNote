using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto
{
    public class FolderDto : KntModelBase
    {
        public Guid FolderId { get; set; }
        public int FolderNumber { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime ModificationDateTime { get; set; }
        public string Name { get; set; }
        public string Tags { get; set; }
        public string PathFolder { get; set; }
        public int Order { get; set; }
        public string OrderNotes { get; set; }
        public string Script { get; set; }
        public Guid? ParentId { get; set; }
        // TODO: Icon es provisional, 
        public string Icon { get; set; } = "fa-folder";
        public List<FolderInfoDto> ChildFolders { get; set; } = new List<FolderInfoDto>();
    }
}
