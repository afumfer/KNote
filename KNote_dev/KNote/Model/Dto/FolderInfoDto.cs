using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class FolderInfoDto : KntModelBase
    {
        public Guid FolderId { get; set; }

        public int FolderNumber { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime ModificationDateTime { get; set; }

        [Required(ErrorMessage = "* Attribute {0} is required ")]
        [MaxLength(256)]
        public string Name { get; set; }

        [MaxLength(1024)]
        public string Tags { get; set; }

        public int Order { get; set; }

        [MaxLength(256)]
        public string OrderNotes { get; set; }

        public string Script { get; set; }

        public Guid? ParentId { get; set; }

    }
}
