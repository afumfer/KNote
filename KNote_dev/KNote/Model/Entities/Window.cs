using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using KNote.Model;


namespace KNote.Model.Entities
{
    public class Window : ModelBase
    {
        #region Constructor

        public Window() : base() { }
        
        #endregion

        #region Property definitions

        private Guid _windowId;
        [Key]
        public Guid WindowId
        {
            get { return _windowId; }
            set
            {
                if (_windowId != value)
                {
                    _windowId = value;
                    OnPropertyChanged("WindowId");
                }
            }
        }

        private Guid _noteId;        
        public Guid NoteId
        {
            get { return _noteId; }
            set
            {
                if (_noteId != value)
                {
                    _noteId = value;
                    OnPropertyChanged("NoteId");
                }
            }
        }

        private Guid _userId;        
        public Guid UserId
        {
            get { return _userId; }
            set
            {
                if (_userId != value)
                {
                    _userId = value;
                    OnPropertyChanged("UserId");
                }
            }
        }

        // info del host / entorno del usuario 
        private string _host;
        public string Host
        {
            get { return _host; }
            set
            {
                if (_host != value)
                {
                    _host = value;
                    OnPropertyChanged("Host");
                }
            }
        }

        private bool _visible;
        public bool Visible
        {
            get { return _visible; }
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    OnPropertyChanged("Visible");
                }
            }
        }

        private bool _alwaysOnTop;
        public bool AlwaysOnTop
        {
            get { return _alwaysOnTop; }
            set
            {
                if (_alwaysOnTop != value)
                {
                    _alwaysOnTop = value;
                    OnPropertyChanged("AlwaysOnTop");
                }
            }
        }

        private int _posX;
        public int PosX
        {
            get { return _posX; }
            set
            {
                if (_posX != value)
                {
                    _posX = value;
                    OnPropertyChanged("PosX");
                }
            }
        }

        private int _posY;
        public int PosY
        {
            get { return _posY; }
            set
            {
                if (_posY != value)
                {
                    _posY = value;
                    OnPropertyChanged("PosY");
                }
            }
        }

        private int _width;
        public int Width
        {
            get { return _width; }
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged("Width");
                }
            }
        }

        private int _height;
        public int Height
        {
            get { return _height; }
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged("Height");
                }
            }
        }

        private string _fontName;
        public string FontName
        {
            get { return _fontName; }
            set
            {
                if (_fontName != value)
                {
                    _fontName = value;
                    OnPropertyChanged("FontName");
                }
            }
        }

        private byte _fontSize;
        public byte FontSize
        {
            get { return _fontSize; }
            set
            {
                if (_fontSize != value)
                {
                    _fontSize = value;
                    OnPropertyChanged("FontSize");
                }
            }
        }

        private bool _fontBold;
        public bool FontBold
        {
            get { return _fontBold; }
            set
            {
                if (_fontBold != value)
                {
                    _fontBold = value;
                    OnPropertyChanged("FontBold");
                }
            }
        }

        private bool _fontItalic;
        public bool FontItalic
        {
            get { return _fontItalic; }
            set
            {
                if (_fontItalic != value)
                {
                    _fontItalic = value;
                    OnPropertyChanged("FontItalic");
                }
            }
        }

        private bool _fontUnderline;
        public bool FontUnderline
        {
            get { return _fontUnderline; }
            set
            {
                if (_fontUnderline != value)
                {
                    _fontUnderline = value;
                    OnPropertyChanged("FontUnderline");
                }
            }
        }

        private bool _fontStrikethru;
        public bool FontStrikethru
        {
            get { return _fontStrikethru; }
            set
            {
                if (_fontStrikethru != value)
                {
                    _fontStrikethru = value;
                    OnPropertyChanged("FontStrikethru");
                }
            }
        }

        private int _foreColor;
        public int ForeColor
        {
            get { return _foreColor; }
            set
            {
                if (_foreColor != value)
                {
                    _foreColor = value;
                    OnPropertyChanged("ForeColor");
                }
            }
        }

        private int _titleColor;
        public int TitleColor
        {
            get { return _titleColor; }
            set
            {
                if (_titleColor != value)
                {
                    _titleColor = value;
                    OnPropertyChanged("TitleColor");
                }
            }
        }

        private int _textTitleColor;
        public int TextTitleColor
        {
            get { return _textTitleColor; }
            set
            {
                if (_textTitleColor != value)
                {
                    _textTitleColor = value;
                    OnPropertyChanged("TextTitleColor");
                }
            }
        }

        private int _noteColor;
        public int NoteColor
        {
            get { return _noteColor; }
            set
            {
                if (_noteColor != value)
                {
                    _noteColor = value;
                    OnPropertyChanged("NoteColor");
                }
            }
        }

        private int _textNoteColor;
        public int TextNoteColor
        {
            get { return _textNoteColor; }
            set
            {
                if (_textNoteColor != value)
                {
                    _textNoteColor = value;
                    OnPropertyChanged("TextNoteColor");
                }
            }
        }

        #region Virtual - navigation properties

        private Note _note;        
        public virtual Note Note
        {
            get { return _note; }
            set
            {
                if (_note != value)
                {
                    _note = value;
                    OnPropertyChanged("Note");
                }
            }
        }

        private User _user;
        public virtual User User
        {
            get { return _user; }
            set
            {
                if (_user != value)
                {
                    _user = value;
                    OnPropertyChanged("User");
                }
            }
        }

        #endregion 

        #endregion

        #region Validations

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // ---
            // Capturar las validaciones implementadas vía atributos.
            // ---

            //Validator.TryValidateProperty(this.Name,
            //   new ValidationContext(this, null, null) { MemberName = "Name" },
            //   results);

            //----
            // Validaciones específicas
            //----

            // ---- Ejemplo
            //if (ModificationDateTime < CreationDateTime)
            //{
            //    results.Add(new ValidationResult
            //     ("KMSG: La fecha de modificación no puede ser mayor que la fecha de creación"
            //     , new[] { "ModificationDateTime", "CreationDateTime" }));
            //}

            // ---
            // Retornar List<ValidationResult>()
            // ---           

            return results;
        }


        #endregion
    }
}
