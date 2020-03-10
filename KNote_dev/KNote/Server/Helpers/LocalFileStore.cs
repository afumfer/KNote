using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<string> EditFile(byte[] content, string extension, string container, string pathFile)
        {
            if (!string.IsNullOrEmpty(pathFile))
            {
                await DeleteFile(pathFile, container);
            }

            return await SaveFile(content, extension, container);
        }

        public Task DeleteFile(string path, string container)
        {
            var filename = Path.GetFileName(path);
            string fileDirectory = Path.Combine(env.WebRootPath, container, filename);
            if (File.Exists(fileDirectory))
            {
                File.Delete(fileDirectory);
            }

            return Task.FromResult(0);
        }

        public async Task<string> SaveFile(byte[] content, string extension, string container)
        {
            var filename = $"{Guid.NewGuid()}.{extension}";
            string folder = Path.Combine(env.WebRootPath, container);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string pathSaved = Path.Combine(folder, filename);
            await File.WriteAllBytesAsync(pathSaved, content);

            var actualUrl = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}";
            var pathForBD = Path.Combine(actualUrl, container, filename);
            return pathForBD;
        }
    }
}
