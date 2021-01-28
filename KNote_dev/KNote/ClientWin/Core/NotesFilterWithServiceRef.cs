using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KNote.Model.Dto;

namespace KNote.ClientWin.Core
{
    public class NotesFilterWithServiceRef
    {
        public NotesFilterDto NotesFilter { get; set; }
        public ServiceRef ServiceRef { get; set; }
    }
}
