using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class NoteInfoDto : KntModelBase
    {
        public Guid NoteId { get; set; }

        public int NoteNumber { get; set; }

        [Required(ErrorMessage = "* Attribute {0} is required ")]
        [MaxLength(1024)]
        public string Topic { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime ModificationDateTime { get; set; }

        public string Description { get; set; }

        [MaxLength(1024)]
        public string ContentType { get; set; }

        [MaxLength(1024)]
        public string Tags { get; set; }

        public int Priority { get; set; }

        [Required(ErrorMessage = "* Folder is required ")]
        public Guid? FolderId { get; set; }

        public Guid? NoteTypeId { get; set; }
    }
}
