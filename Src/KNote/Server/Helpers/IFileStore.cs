using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNote.Server.Helpers
{
    public interface IFileStore
    {
        Task<string> EditFile(string contentBase64, string extension, string container, string path, Guid noteId);
        Task DeleteFile(string path, string container, Guid noteId);
        Task<string> SaveFile(string contentBase64, string extension, string container, Guid noteId);
        string GetRelativeUrl(string filename, string container, Guid noteId);
    }
}
