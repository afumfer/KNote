using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KNote.Server.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using KNote.Service.Core;

namespace KNote.Server.Controllers
{
    // TODO: Diseñar e implementar el API que mantiene los parámetros del sistema

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SystemValuesController : ControllerBase
    {
        private IKntService _service { get; set; }
        private readonly AppSettings _appSettings;

        public SystemValuesController(IKntService service, IOptions<AppSettings> appSettings)
        {
            _service = service;
            _appSettings = appSettings.Value;
        }

        [HttpGet]    // GET api/users
        public IActionResult Get()
        {
            return null;
        }
    }

}
