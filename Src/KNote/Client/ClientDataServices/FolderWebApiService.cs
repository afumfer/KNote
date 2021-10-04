using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace KNote.Client.ClientDataServices
{
    public class FolderWebApiService : IFolderWebApiService
    {
        private readonly HttpClient _httpClient;

        public FolderWebApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
    }
}
