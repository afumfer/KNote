using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KNote.Model.Dto
{
    public class NotesFilterDto : NotesSearchDto
    {        
        public int TotalPages { get; set; } = 0;
       
        public Guid? FolderId { get; set; }
        public string Topic { get; set; }
        public Guid? NoteTypeId { get; set; }
        public string Tags { get; set; }        
        public string Description { get; set; }

        public List<AtrFilterDto> AttributesFilter { get; set; } = new List<AtrFilterDto>();
    }

    public class AtrFilterDto
    {
        private const string KMSG = "Attribute {0} is required. ";

        [Required(ErrorMessage = KMSG)]
        public Guid AtrId { get; set; }

        public string AtrName { get; set; }
        
        [Required(ErrorMessage = KMSG)]
        public string Value { get; set; }
    }

}
