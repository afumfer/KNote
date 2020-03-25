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

        public async Task<string> EditFile(string contentBase64, string extension, string container, string pathFile)
        {
            if (!string.IsNullOrEmpty(pathFile))
            {
                await DeleteFile(pathFile, container);
            }

            return await SaveFile(contentBase64, extension, container);
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

        public async Task<string> SaveFile(string contentBase64, string filename, string container)
        {
            var folder = Path.Combine(env.WebRootPath, container);
            var content = Convert.FromBase64String(contentBase64);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string pathSaved = Path.Combine(folder, filename);
            await File.WriteAllBytesAsync(pathSaved, content);

            // TODO: arreglar path /\

            var actualUrl = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}";
            var fullUrl = Path.Combine(actualUrl, container, filename);
            return fullUrl.Replace(@"\", @"/");
        }

        public string GetFullUrl(string filename, string container)
        {
            // TODO: arreglar path /\
            var actualUrl = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}";
            var fullUrl = Path.Combine(actualUrl, container, filename);
            return fullUrl.Replace(@"\", @"/");
        }

        //public async Task<string> SaveFile(byte[] content, string filename, string container)
        //{            
        //    string folder = Path.Combine(env.WebRootPath, container);

        //    if (!Directory.Exists(folder))
        //    {
        //        Directory.CreateDirectory(folder);
        //    }

        //    string pathSaved = Path.Combine(folder, filename);
        //    await File.WriteAllBytesAsync(pathSaved, content);

        //    // TODO: !!! Aquí falta el nombre de la subaplicación IIS e invertir las barras de las rutas \

        //    var actualUrl = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}/{httpContextAccessor.HttpContext.Request.PathBase}";
        //    var fullPath = Path.Combine(actualUrl, container, filename);
        //    return fullPath;
        //}

        //public async Task<string> SaveFile2(byte[] content, string extension, string container)
        //{
        //    var filename = $"{Guid.NewGuid()}.{extension}";
        //    string folder = Path.Combine(env.WebRootPath, container);

        //    if (!Directory.Exists(folder))
        //    {
        //        Directory.CreateDirectory(folder);
        //    }

        //    string pathSaved = Path.Combine(folder, filename);
        //    await File.WriteAllBytesAsync(pathSaved, content);

        //    var actualUrl = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}";
        //    var pathForBD = Path.Combine(actualUrl, container, filename);
        //    return pathForBD;
        //}
    }
}
