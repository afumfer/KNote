using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class KAttributeDto : KAttributeInfoDto
    {
        public NoteTypeDto NoteTypeDto { get; set; }

        public List<KAttributeTabulatedValueDto> KAttributeValues { get; set; } = new List<KAttributeTabulatedValueDto>();        
    }
}
