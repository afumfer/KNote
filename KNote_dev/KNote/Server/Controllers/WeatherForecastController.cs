using KNote.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KNote.DomainModel.Services;
using KNote.Shared.Dto;
using KNote.Shared.Dto.Info;
using Microsoft.Extensions.Configuration;
using KNote.Server.Helpers;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

namespace KNote.Server.Controllers
{
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
        private readonly AppSettings appSettings;


        public WeatherForecastController(ILogger<WeatherForecastController> logger, IKntService service, IOptions<AppSettings> appSettings)
        {
            this.logger = logger;
            this.service = service;
            this.appSettings = appSettings.Value;
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
        public IEnumerable<FolderInfoDto> GetFolders()
        {            
            return service.Folders.GetAll().Entity;
        }

        [HttpGet("GetAppSettings")]
        public IEnumerable<string> GetAppSettings()
        {
            string[] values = new string[] 
            { 
                "aa",
                appSettings.Version, 
                appSettings.ApplicationName 
            };
            return values;
        }


    }

}
