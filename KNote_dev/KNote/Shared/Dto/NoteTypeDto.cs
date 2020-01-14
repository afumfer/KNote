using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KNote.Shared.Dto
{
    public class NoteTypeDto : KntModelBase
    {
        public Guid NoteTypeId { get; set; }
        [Required(ErrorMessage = "* Attribute {0} is required ")]
        public string Key { get; set; }
        [Required(ErrorMessage = "* Attribute {0} is required ")]
        public string Name { get; set; }
    }
}
