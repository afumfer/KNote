using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNote.Server.Helpers
{
    public interface IFileStore
    {        
        Task DeleteFile(string path, string container);
        Task<string> SaveFile(string contentBase64, string filename, string container);
        Task<string> SaveFile(byte[] contentArrayBytes, string filename, string container);
        string GetRelativeUrl(string filename, string container);
        string GetResourcesContainerRootUrl();
        string GetResourcesContainerRootPath();
    }
}
