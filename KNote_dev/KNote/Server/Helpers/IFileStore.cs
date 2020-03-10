using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNote.Server.Helpers
{
    public interface IFileStore
    {
        Task<string> EditFile(byte[] content, string extension, string container, string path);
        Task DeleteFile(string path, string container);
        Task<string> SaveFile(byte[] content, string extension, string container);
    }
}
