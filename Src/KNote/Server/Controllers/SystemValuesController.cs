using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using KNote.Service.Core;
using KNote.Server.Helpers;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System;

namespace KNote.Server.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class SystemValuesController : ControllerBase
{
    private readonly IKntService _service;
    private readonly AppSettings _appSettings;
    private readonly ILogger<SystemValuesController> _logger;

    public SystemValuesController(IKntService service, IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> appSettings, ILogger<SystemValuesController> logger)
    {
        _service = service;
        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    [HttpGet]    // GET api/users
    public IActionResult Get()
    {
        return null;
    }

    [HttpGet("GetAppSettings")]
    public IEnumerable<string> GetAppSettings()
    {
        string[] values = new string[]
        {            
            $"ActivateMessageBroker: {_appSettings.ActivateMessageBroker}",
            $"MountResourceContainerOnStartup: {_appSettings.MountResourceContainerOnStartup}"            
        };

        _logger.LogTrace("GetAppSettings at {dateTime}.", DateTime.Now);

        return values;
    }
}
