using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto
{
    public class FolderInfoDto : KntModelBase
    {
        public Guid FolderId { get; set; }
        public int FolderNumber { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime ModificationDateTime { get; set; }
        public string Name { get; set; }
        public string Tags { get; set; }
        public string PathFolder { get; set; }
        public int Order { get; set; }
        public string OrderNotes { get; set; }
        public string Script { get; set; }
        public Guid? ParentId { get; set; }
        // TODO: Icon es provisional, 
        public string Icon { get; set; } = "fa-folder";
        public List<FolderInfoDto> ChildFolders { get; set; } = new List<FolderInfoDto>();

        public string ShortName
        {
            get
            {
                if (Name.Length > 45)
                    return Name.Substring(0, 42) + "...";
                else
                    return Name;
            }
        }

        private bool _expanded = false;
        public bool Expanded
        {
            get { return _expanded; }
        }

        public void Toggle()
        {
            _expanded = !_expanded;
        }

        public string getIcon()
        {
            if (_expanded)
            {
                //return "-";
                return "oi oi-caret-bottom";
                //return "oi oi-chevron-bottom";
            }

            //return "+";
            return "oi oi-caret-right";
            //return "oi oi-chevron-right";
        }

        public bool Selected { get; set; } = false;

        public string getColor()
        {
            if (Selected)
                return "text-light bg-dark";
            return "text-dark";

        }
    }
}
