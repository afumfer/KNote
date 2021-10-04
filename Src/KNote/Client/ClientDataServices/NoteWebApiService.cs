using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace KNote.Client.ClientDataServices
{
    public class NoteWebApiService : INoteWebApiService
    {
        private readonly HttpClient _httpClient;

        public NoteWebApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
    }
}
