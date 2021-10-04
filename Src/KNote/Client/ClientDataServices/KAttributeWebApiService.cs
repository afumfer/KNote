using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace KNote.Client.ClientDataServices
{
    public class KAttributeWebApiService : IKAttributeWebApiService
    {
        private readonly HttpClient _httpClient;

        public KAttributeWebApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
    }
}
