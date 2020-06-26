﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class TraceNoteTypeDto : KntModelBase
    {
        public Guid TraceNoteTypeId { get; set; }

        [Required(ErrorMessage = "* Attribute {0} is required ")]
        [MaxLength(256)]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}