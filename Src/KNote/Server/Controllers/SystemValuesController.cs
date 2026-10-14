using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using KNote.Service.Core;
using KNote.Server.Helpers;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using KNote.Model;

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
        _service.Logger = logger;
        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return null;
    }

    // The repository's name (DB_REPOSITORYNAME), shown in the header of the Web, also on its public Home.
    [AllowAnonymous]
    [HttpGet("[action]")]
    public async Task<IActionResult> RepositoryName()
    {
        try
        {
            var resService = await _service.SystemValues.GetRepositoryInfoAsync();

            var resApi = new Result<string>(resService.Entity?.Name);
            resApi.AddListErrorMessage(resService.ListErrorMessage);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RepositoryName at {dateTime}.", DateTime.Now);
            var resApi = new Result<string>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
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
