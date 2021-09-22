using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace KNote.Client.ClientDataServices
{
    public class WebApiService : IWebApiService
    {
        private readonly HttpClient _httpClient;

        public WebApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private IUserWebApiService _users;
        public IUserWebApiService Users
        {
            get
            {
                if (_users == null)
                    _users = new UserWebApiService(_httpClient);
                return _users;
            }
        }

    }
}
