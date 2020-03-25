using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNote.Server.Helpers
{
    public interface IFileStore
    {
        Task<string> EditFile(string contentBase64, string extension, string container, string path);
        Task DeleteFile(string path, string container);
        Task<string> SaveFile(string contentBase64, string extension, string container);
        string GetFullUrl(string filename, string container);
    }
}
