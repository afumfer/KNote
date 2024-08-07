﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace KNote.Server.Helpers;

public class LocalFileStore : IFileStore
{
    private readonly IWebHostEnvironment env;
    private readonly IHttpContextAccessor httpContextAccessor;        

    public LocalFileStore(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        this.env = env;
        this.httpContextAccessor = httpContextAccessor;
    }
    
    public Task DeleteFile(string path, string container)
    {             
        var filename = Path.GetFileName(path);
        string fileDirectory = Path.Combine(GetResourcesContainerRootPath(), container, filename);
        if (File.Exists(fileDirectory))            
            File.Delete(fileDirectory);            

        return Task.CompletedTask;
    }

    public async Task<string> SaveFile(string contentBase64, string filename, string container)
    {                                    
        var content = Convert.FromBase64String(contentBase64);
        return await SaveFile(content, filename, container);
    }

    public async Task<string> SaveFile(byte[] content, string filename, string container)
    {
        var folder = Path.Combine(GetResourcesContainerRootPath(), container);            

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string pathSaved = Path.Combine(folder, filename);
        if (!File.Exists(pathSaved))
            await File.WriteAllBytesAsync(pathSaved, content);

        var actualUrl = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}";
        var fullUrl = Path.Combine(actualUrl, container, filename);
        return fullUrl.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    public string GetRelativeUrl(string filename, string container)
    {            
        var relativeUrl = Path.Combine(container, filename);
        return relativeUrl.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    public string GetResourcesContainerRootUrl()
    {
        return $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}";
    }

    public string GetResourcesContainerRootPath()
    {                       
        return env.WebRootPath;            
    }
}
