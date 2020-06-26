using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class KAttributeDto : KntModelBase
    {
        public Guid KAttributeId { get; set; }
        
        [Required(ErrorMessage = "* Attribute {0} is required ")]
        [MaxLength(256)]
        public string Name { get; set; }

        public string Description { get; set; }

        public bool RequiredValue { get; set; }

        public int Order { get; set; }        

        public bool Disabled { get; set; }

        public string Script { get; set; }

        public EnumKAttributeDataType KAttributeDataType { get; set; }

        public Guid? NoteTypeId { get; set; }

        public NoteTypeDto NoteTypeDto { get; set; }

        public List<KAttributeTabulatedValueDto> KAttributeValues { get; set; } = new List<KAttributeTabulatedValueDto>();        
    }
}
