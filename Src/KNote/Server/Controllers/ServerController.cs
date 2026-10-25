using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Local;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace KNote.Server.Controllers;

/// <summary>What this server can do (its configuration), for the web app to show or hide it. Anonymous.</summary>
[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class ServerController : ControllerBase
{
    private readonly IServiceProvider _services;

    public ServerController(IServiceProvider services)
    {
        _services = services;
    }

    [HttpGet("capabilities")]
    public IActionResult Capabilities() =>
        Ok(new Result<ServerCapabilitiesDto>(new ServerCapabilitiesDto { LocalAdmin = _services.GetService<KntLocalData>() != null }));
}
