using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto
{
    public class ResourceDto : KntModelBase
    {
        public Guid ResourceId { get; set; }

        [Required(ErrorMessage = "* Attribute {0} is required ")]
        [MaxLength(1024)]
        public string Path { get; set; }

        public string FullPath { get; set; }

        public string Description { get; set; }

        public int Order { get; set; }

        [MaxLength(64)]
        public string FileMimeType { get; set; }

        public bool ContentInDB { get; set; }

        public byte[] ContentDB { get; set; }
        
        public string ContentBase64 { get; set; }

        public Guid NoteId { get; set; }
    }
}