using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto
{
    public class NoteDto : KntModelBase
    {
        public Guid NoteId { get; set; }

        public int NoteNumber { get; set; }

        [Required(ErrorMessage = "* Attribute {0} is required ")]
        public string Topic { get; set; }

        public string Description { get; set; }

        public string ContentType { get; set; }        

        public string Tags { get; set; }

        public int Priority { get; set; }

        public Guid FolderId { get; set; }

        public Guid? NoteTypeId { get; set; }

        public List<NoteKAttributeDto> KAttributes { get; set; } = new List<NoteKAttributeDto>();

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
