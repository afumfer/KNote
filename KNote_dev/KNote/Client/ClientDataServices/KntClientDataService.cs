using KNote.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNote.Client.ClientDataServices
{
    public class KntClientDataService : IKntClientDataService
    {
        public List<NoteInfoDto> GetNotes()
        {
            return new List<NoteInfoDto>()
    {
            new NoteInfoDto() { Topic = "My personal note 1", NoteNumber = 1 },
            new NoteInfoDto() { Topic = "My personal note 2", NoteNumber = 2 },
            new NoteInfoDto() { Topic = "My personal note 3", NoteNumber = 3 }
        };
        }
    }
}
