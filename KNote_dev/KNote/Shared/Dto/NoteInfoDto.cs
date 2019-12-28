using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto
{
    public class NoteInfoDto : KntModelBase
    {
        public Guid NoteId { get; set; }
        public int NoteNumber { get; set; }
        public string Topic { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime ModificationDateTime { get; set; }
        public string Description { get; set; }
        public string ContentType { get; set; }
        public string Script { get; set; }
        public string InternalTags { get; set; }
        public string Tags { get; set; }
        public int Priority { get; set; }
        public Guid FolderId { get; set; }
        public Guid? NoteTypeId { get; set; }

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

        //public string ShortTopic
        //{
        //    get
        //    {
        //        if (string.IsNullOrWhiteSpace(Topic))
        //            return null;
        //        if (Topic.Length > 60)
        //            return Topic.Substring(0, 60) + "...";
        //        else
        //            return Topic;
        //    }
        //}

    }
}
