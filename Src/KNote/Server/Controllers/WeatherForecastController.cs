using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Helpers;
using KNote.Service.Core;

namespace KNote.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> logger;
    IKntService service;
    
    public WeatherForecastController(ILogger<WeatherForecastController> logger, IKntService service)
    {
        this.logger = logger;
        this.service = service;        
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {           
        var rng = new Random();
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = rng.Next(-20, 55),
            Summary = Summaries[rng.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpGet("GetFolders")]
    public async Task<IEnumerable<FolderInfoDto>> GetFolders()
    {            
        return (await service.Folders.GetAllAsync()).Entity;
    }

    [HttpGet("GetFoldersFull")]
    public async Task<IEnumerable<FolderInfoDto>> GetFoldersFull()
    {
        return (await service.Folders.GetTreeAsync()).Entity;
    }

}
