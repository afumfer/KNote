using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Service;
using KNote.Service.Services;

namespace KNote.ClientWin.Core
{
    public class ServiceWithNoteId
    {
        public IKntService Service { get; set; }
        public Guid NoteId { get; set; }

    }
}
