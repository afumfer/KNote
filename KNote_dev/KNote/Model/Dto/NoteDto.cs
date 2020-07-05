using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class NoteDto : NoteInfoDto
    {
        public FolderDto FolderDto { get; set; } = new FolderDto();

        public List<NoteKAttributeDto> KAttributesDto { get; set; } = new List<NoteKAttributeDto>();

        public bool IsNew { get; set; } = false;

        // TODO: Eliminar la siguiente propiedad, se deberá implementar en ContentType
        public bool HtmlFormat
        {
            get
            {
                if (Description == null || Description.Length < 5)
                    return false;

                var tmp = Description.Substring(0, 5);
                return (tmp == "<BODY") ? true : false;
            }

            set { }
        }
    }
}
