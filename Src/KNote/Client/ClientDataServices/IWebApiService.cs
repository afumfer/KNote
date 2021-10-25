using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNote.Client.ClientDataServices
{
    public interface IWebApiService
    {
        IUserWebApiService Users { get; }
        INoteTypeWebApiService NoteTypes { get; }
        IKAttributeWebApiService KAttributes { get; }
        IFolderWebApiService Folders { get; }
        INoteWebApiService Notes { get; }
    }
}
