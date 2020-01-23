using KNote.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KNote.Client.ClientDataServices
{
    public class KntClientDataService : IKntClientDataService
    {
        private readonly HttpClient httpClient;

        public KntClientDataService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<HttpResponseWrapper<object>> Post<T>(string url, T enviar)
        {
            var sendJSON = JsonSerializer.Serialize(enviar);
            var sendContent = new StringContent(sendJSON, Encoding.UTF8, "application/json");
            var responseHttp = await httpClient.PostAsync(url, sendContent);
            return new HttpResponseWrapper<object>(null, !responseHttp.IsSuccessStatusCode, responseHttp);
        }


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
