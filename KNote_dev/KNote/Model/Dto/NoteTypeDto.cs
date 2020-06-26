using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KNote.Model.Dto
{
    public class NoteTypeDto : KntModelBase
    {
        public Guid NoteTypeId { get; set; }

        [Required(ErrorMessage = "* Attribute {0} is required ")]
        [MaxLength(256)]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
