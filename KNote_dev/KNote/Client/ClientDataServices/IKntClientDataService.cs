using KNote.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNote.Client.ClientDataServices
{
    public interface IKntClientDataService
    {
        List<NoteInfoDto> GetNotes();
        Task<HttpResponseWrapper<object>> Post<T>(string url, T enviar);
    }
}
