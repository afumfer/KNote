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

        private INoteTypeWebApiService _noteTypes;
        public INoteTypeWebApiService NoteTypes
        {
            get
            {
                if (_noteTypes == null)
                    _noteTypes = new NoteTypeWebApiService(_httpClient);
                return _noteTypes;
            }
        }

        private IKAttributeWebApiService _kAttributes;
        public IKAttributeWebApiService KAttributes
        {
            get
            {
                if (_kAttributes == null)
                    _kAttributes = new KAttributeWebApiService(_httpClient);
                return _kAttributes;
            }
        }


        private IFolderWebApiService _folders;
        public IFolderWebApiService Folders
        {
            get
            {
                if (_folders == null)
                    _folders = new FolderWebApiService(_httpClient);
                return _folders;
            }
        }


        private INoteWebApiService _notes;
        public INoteWebApiService Notes
        {
            get
            {
                if (_notes == null)
                    _notes = new NoteWebApiService(_httpClient);
                return _notes;
            }
        }

    }
}
