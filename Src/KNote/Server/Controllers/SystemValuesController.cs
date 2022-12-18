using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KNote.Server.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using KNote.Service.Core;
using Microsoft.AspNetCore.Http;

namespace KNote.Server.Controllers
{
    // TODO: Diseñar e implementar el API que mantiene los parámetros del sistema

    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class SystemValuesController : ControllerBase
    {
        private readonly IKntService _service;

        public SystemValuesController(IKntService service, IHttpContextAccessor httpContextAccessor)
        {
            _service = service;
            _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
        }

        [HttpGet]    // GET api/users
        public IActionResult Get()
        {
            return null;
        }
    }

}
