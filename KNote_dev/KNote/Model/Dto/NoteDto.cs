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
        private FolderDto _folderDto;
        public FolderDto FolderDto 
        {
            get {
                if (_folderDto == null)
                    _folderDto = new FolderDto();
                return _folderDto; 
            }
            set
            {
                if (_folderDto != value)
                {
                    _folderDto = value;
                    OnPropertyChanged("FolderDto");
                }
            }
        }

        private NoteTypeDto _noteTypeDto;
        public NoteTypeDto NoteTypeDto
        {
            get
            {
                if (_noteTypeDto == null)
                    _noteTypeDto = new NoteTypeDto();
                return _noteTypeDto;
            }
            set
            {
                if (_noteTypeDto != value)
                {
                    _noteTypeDto = value;
                    OnPropertyChanged("NoteTypeDto");}
            }
        }

        private List<NoteKAttributeDto> _kAttributesDto;
        public List<NoteKAttributeDto> KAttributesDto
        {
            get {
                if (_kAttributesDto == null)
                    _kAttributesDto = new List<NoteKAttributeDto>();
                return _kAttributesDto; 
            }
            set
            {
                if (_kAttributesDto != value)
                {
                    _kAttributesDto = value;
                    OnPropertyChanged("KAttributesDto");
                }
            }
        } 
        
        // TODO: Eliminar la siguiente propiedad, se deberá implementar en ContentType
        public bool HtmlFormat
        {
            get
            {
                if (Description == null || Description.Length < 5)
                    return false;
                
                if (ContentType == "html")
                    return true;

                var tmp = Description.Substring(0, 5);
                if (tmp == "<BODY")
                {
                    ContentType = "html";
                    return true;
                }
                else
                    return false;
            }

            set { }
        }
    }
}
