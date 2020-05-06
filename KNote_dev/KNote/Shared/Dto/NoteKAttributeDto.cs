using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto
{
    public class NoteKAttributeDto : KntModelBase
    {
        public Guid NoteKAttributeId { get; set; }
        public Guid NoteId { get; set; }
        public Guid KAttributeId { get; set; }
        public string Value { get; set; }
        public string KAttributeName { get; set; }        
        public string Description { get; set; }        
        public EnumKAttributeDataType KAttributeDataType { get; set; }
        public Guid? KAttributeNoteTypeId { get; set; }
        public bool RequiredValue { get; set; }
        public int Order { get; set; }
        public string Script { get; set; }
        public bool Disabled { get; set; }

        // For view
        public string ValueString { get; set; }
        public DateTime? ValueDateTime { get; set; }
        public int? ValueInt { get; set; }
        public double? ValueDouble { get; set; }
        public bool ValueBool { get; set; }
        public string ValueTabulate { get; set; }
        public string ValueTags { get; set; }

        public List<KAttributeTabulatedValueDto> TabulatedValues = new List<KAttributeTabulatedValueDto>();
        public List<MultiSelectListDto> TagsValues = new List<MultiSelectListDto>();

    }
}