using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KNote.DomainModel.Services;
using KNote.Server.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace KNote.Server.Controllers
{
    [Authorize]
    //[Authorize(Roles = "Administrators")]
    [ApiController]
    [Route("api/[controller]")]
    public class KAttributesController : ControllerBase
    {
        private IKntService _service { get; set; }
        private readonly AppSettings _appSettings;

        public KAttributesController(IKntService service, IOptions<AppSettings> appSettings)
        {
            _service = service;
            _appSettings = appSettings.Value;

        }


        [HttpGet]    // GET api/kattributes      
        public IActionResult Get()
        {
            return null;
        }

    }

}
