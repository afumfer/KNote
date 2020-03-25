﻿using System;
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
        public string Name { get; set; }

        public string Container { get; set; }

        public string FullUrl { get; set; }

        public string Description { get; set; }

        public int Order { get; set; }

        [MaxLength(64)]
        public string FileType { get; set; }

        public bool ContentInDB { get; set; }

        public byte[] ContentArrayBytes { get; set; }
        
        public string ContentBase64 { get; set; }

        public Guid NoteId { get; set; }
    }
}