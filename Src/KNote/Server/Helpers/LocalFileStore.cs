using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KNote.Model;

namespace KNote.Server.Helpers
{
    public class LocalFileStore : IFileStore
    {
        private readonly IWebHostEnvironment env;
        private readonly IHttpContextAccessor httpContextAccessor;        

        public LocalFileStore(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            this.env = env;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> EditFile(string contentBase64, string extension, string container, string pathFile)
        {
            if (!string.IsNullOrEmpty(pathFile))
                await DeleteFile(pathFile, container);

            return await SaveFile(contentBase64, extension, container);
        }
        
        public Task DeleteFile(string path, string container)
        {             
            var filename = Path.GetFileName(path);
            string fileDirectory = Path.Combine(GetContainerResourcesPath(), container, filename);
            if (File.Exists(fileDirectory))            
                File.Delete(fileDirectory);            

            return Task.FromResult(0);
        }

        public async Task<string> SaveFile(string contentBase64, string filename, string container)
        {                        
            var folder = Path.Combine(GetContainerResourcesPath(), container);
            var content = Convert.FromBase64String(contentBase64);
                                    
            if (!Directory.Exists(folder))            
                Directory.CreateDirectory(folder);
                        
            string pathSaved = Path.Combine(folder, filename);
            if (!File.Exists(pathSaved))
                await File.WriteAllBytesAsync(pathSaved, content);
           
            var actualUrl = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}";
            var fullUrl = Path.Combine(actualUrl, container, filename);
            return fullUrl.Replace(@"\", @"/");
        }

        public string GetRelativeUrl(string filename, string container)
        {            
            var relativeUrl = Path.Combine(container, filename);
            return relativeUrl.Replace(@"\", @"/");
        }

        private string GetContainerResourcesPath()
        {            
            // TODO: !!! get from RepositortRef values
            return env.WebRootPath;            
        }
    }
}
