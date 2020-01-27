using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto
{
    public class FolderDto : KntModelBase
    {
        public Guid FolderId { get; set; }

        public int FolderNumber { get; set; }

        //public DateTime CreationDateTime { get; set; }
        //public DateTime ModificationDateTime { get; set; }
        
        [Required(ErrorMessage = "* Attribute {0} is required ")]
        [MaxLength(256)]
        public string Name { get; set; }

        [MaxLength(1024)]
        public string Tags { get; set; }

        [MaxLength(400)]
        public string PathFolder { get; set; }

        public int Order { get; set; }

        [MaxLength(256)]
        public string OrderNotes { get; set; }

        public string Script { get; set; }

        public Guid? ParentId { get; set; }

        // TODO: Icon es provisional, 
        public string Icon { get; set; } = "fa-folder";
        
        public List<FolderInfoDto> ChildFolders { get; set; } = new List<FolderInfoDto>();
    }
}
