using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using KNote.Server.Helpers;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.Logging;
using KNote.Repository.EntityFramework.Entities;

namespace KNote.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IKntService _service;
    private readonly AppSettings _appSettings;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IKntService service, IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> appSettings, ILogger<UsersController> logger)
    {
        _service = service;
        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    [HttpGet]    // GET api/users        
    public async Task<IActionResult> Get([FromQuery] PageIdentifier pagination)
    {
        try
        {            
            pagination.PageNumber = (pagination.PageNumber < 1) ? 1 : pagination.PageNumber;
            pagination.PageSize = (pagination.PageSize < 1) ? 9999 : pagination.PageSize;
            _logger.LogTrace("User pagination {page} get at {dateTime}.", pagination.PageNumber, DateTime.Now);

            var kresApi = await _service.Users.GetAllAsync(pagination);

            if (kresApi.IsValid)
                return Ok(kresApi);
            else
                return BadRequest(kresApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User pagination {page} getPagination at {dateTime}.", pagination.PageNumber, DateTime.Now);
            var kresApi = new Result<List<UserDto>>();
            kresApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(kresApi);
        }
    }

    [HttpGet("{id}")]    // GET api/users/id
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            _logger.LogTrace("User {id} get at {dateTime}.", id, DateTime.Now);

            var resApi = await _service.Users.GetAsync(id);
            if (resApi.IsValid)
                return Ok(resApi);
            else
            {
                return BadRequest(resApi);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User {id} get at {dateTime}", id, DateTime.Now);
            var kresApi = new Result<UserDto>();
            kresApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(kresApi);
        }
    }

    [HttpPost]   // POST api/users
    [HttpPut]    // PUT api/users
    [Authorize(Roles = "Admin")]        
    public async Task<IActionResult> Post([FromBody] UserDto userDto)
    {
        try
        {
            _logger.LogTrace("User {user} post/put at {dateTime}.", userDto.FullName?.ToString(), DateTime.Now);

            var kresApi = await _service.Users.SaveAsync(userDto);
            if (kresApi.IsValid)
                return Ok(kresApi);
            else
                return BadRequest(kresApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User {user} post/put at {dateTime}.", userDto.FullName?.ToString(), DateTime.Now);
            var kresApi = new Result<UserDto>();
            kresApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(kresApi);
        }
    }

    [HttpDelete("{id}")]    // DELETE api/users/id
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            _logger.LogTrace("User {id} delete at {DateTime}.", id, DateTime.Now);

            var kresApi = await _service.Users.DeleteAsync(id);
            if (kresApi.IsValid)
                return Ok(kresApi);
            else
                return BadRequest(kresApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User {id} delete at {dateTime}.", id, DateTime.Now);
            var kresApi = new Result<UserDto>();
            kresApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(kresApi);
        }
    }

    [AllowAnonymous]
    [HttpPost("[action]")]   
    public async Task<IActionResult> Register([FromBody]UserRegisterDto user)
    {            
        try
        {
            _logger.LogTrace("User {user} register at {dateTime}.", user.UserName, DateTime.Now);

            var kresService = await _service.Users.CreateAsync(user);

            if (kresService.IsValid)
            {
                return Ok(BuildToken(kresService.Entity));                    
            }
            else
            {
                _logger.LogWarning("User {user} register BadRequest at {dateTime}.", user.UserName?.ToString(), DateTime.Now);
                return BadRequest(new UserTokenDto { success = kresService.IsValid, token = "", error = kresService.ErrorMessage });
            }                    
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "User {user} register at {dateTime}.", user.UserName?.ToString(), DateTime.Now);
            return BadRequest(new UserTokenDto { success = false, token = "", error = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("[action]")]
    public async Task<IActionResult> Login([FromBody]UserCredentialsDto credentials)
    {            
        try
        {
            _logger.LogTrace("User {user} Login at {dateTime}.", credentials.UserName, DateTime.Now);

            var kresRep = await _service.Users.AuthenticateAsync(credentials);
            var user = kresRep.Entity;

            if (!kresRep.IsValid)
            {
                _logger.LogWarning("User {user} login BadRequest at {dateTime}.", credentials.UserName, DateTime.Now);
                return BadRequest(new UserTokenDto { success = false, token = "", error = kresRep.ErrorMessage });
            }

            return Ok(BuildToken(user));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "User {user} login at {dateTime}.", credentials.UserName, DateTime.Now);
            return BadRequest(new UserTokenDto { success = false, token = "", uid = "", error = ex.Message });
        }
    }

    #region Private methods

    private UserTokenDto BuildToken(UserDto userDto)
    {
        var claims = new List<Claim>()
        {
            //new Claim(JwtRegisteredClaimNames.UniqueName, userDto.EMail),
            //new Claim(ClaimTypes.Name, userDto.UserName),

            new Claim(ClaimTypes.Name, userDto.UserName),
            new Claim(JwtRegisteredClaimNames.UniqueName, userDto.EMail),
                            
            //new Claim("KNoteApp", "KNoteWeb"),

            // Jti es un identificador del toquen, podría ser útil para llevar un registro y en algún 
            // momento poder invalidar dicho token. (Podría servir también como un id de sesión). 
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
                    
        var roles = userDto.RoleDefinition.Split(',');
        foreach (var rol in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, rol.Trim()));
        }

        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiration = DateTime.UtcNow.AddYears(1);

        JwtSecurityToken token = new JwtSecurityToken(
           issuer: null,
           audience: null,
           claims: claims,
           expires: expiration,
           signingCredentials: creds);

        return new UserTokenDto()
        {
            success = true,
            token = new JwtSecurityTokenHandler().WriteToken(token),
            uid = userDto.UserId.ToString()
        };
    }

    #endregion 

}
