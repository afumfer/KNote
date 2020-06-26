using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto.Info
{
    public class WindowInfoDto : KntModelBase 
    {
        public Guid WindowId { get; set; }
        public Guid NoteId { get; set; }
        public Guid UserId { get; set; }
        public bool Visible { get; set; }
        public bool AlwaysOnTop { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string FontName { get; set; }
        public byte FontSize { get; set; }
        public bool FontBold { get; set; }
        public bool FontItalic { get; set; }
        public bool FontUnderline { get; set; }
        public bool FontStrikethru { get; set; }
        public int ForeColor { get; set; }
        public int TitleColor { get; set; }
        public int TextTitleColor { get; set; }
        public int NoteColor { get; set; }
        public int TextNoteColor { get; set; }
    }
}
