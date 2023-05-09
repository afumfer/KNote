using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using KNote.Service.Core;

namespace KNote.Server.Controllers;

// TODO: Design and implement the API that maintains system parameters

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
