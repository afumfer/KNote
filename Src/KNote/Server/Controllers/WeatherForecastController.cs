using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.AspNetCore.Authorization;

namespace KNote.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    IKntService _service;
    
    public WeatherForecastController(IKntService service, ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
        _service = service;        
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {           
        var rng = new Random();
        
        _logger.LogTrace("Get {dateTime}.", DateTime.Now);
        
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
        _logger.LogTrace("GetFolders {dateTime}.", DateTime.Now);
        return (await _service.Folders.GetAllAsync()).Entity;
    }

    [HttpGet("GetFoldersFull")]
    public async Task<IEnumerable<FolderInfoDto>> GetFoldersFull()
    {
        _logger.LogTrace("GetFoldersFull {dateTime}.", DateTime.Now);
        return (await _service.Folders.GetTreeAsync()).Entity;
    }
}
