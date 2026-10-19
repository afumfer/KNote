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
using Microsoft.AspNetCore.RateLimiting;
using KNote.Server.Helpers;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.Logging;

namespace KNote.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IKntService _service;
    private readonly AppSettings _appSettings;
    private readonly KntPasswordStamps _passwordStamps;
    private readonly KntPasswordResetMail _passwordResetMail;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IKntService service, IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> appSettings,
        KntPasswordStamps passwordStamps, KntPasswordResetMail passwordResetMail, ILogger<UsersController> logger)
    {
        _service = service;
        _service.Logger = logger;
        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
        _appSettings = appSettings.Value;
        _passwordStamps = passwordStamps;
        _passwordResetMail = passwordResetMail;
        _logger = logger;
    }

    [HttpGet]
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
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    [HttpGet("{id}")]
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
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    [HttpPost]
    [HttpPut]
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
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    [HttpDelete("{id}")]
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
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
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

            // The new user's role is decided by the service (KntUsersRegisterAsyncCommand), not by the
            // RoleDefinition this anonymous endpoint receives.
            var kresService = await _service.Users.RegisterAsync(user);

            if (kresService.IsValid)
            {
                return Ok(await BuildTokenAsync(kresService.Entity));
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
            return BadRequest(new UserTokenDto { success = false, token = "", error = ex.GetRootMessage() });
        }
    }

    [AllowAnonymous]
    [EnableRateLimiting(KntRateLimits.SignIn)]
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

            return Ok(await BuildTokenAsync(user));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "User {user} login at {dateTime}.", credentials.UserName, DateTime.Now);
            return BadRequest(new UserTokenDto { success = false, token = "", uid = "", error = ex.GetRootMessage() });
        }
    }

    // An Admin sets the password of a user. The sessions that user had open are closed.
    [HttpPut("{id}/password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetPassword(Guid id, [FromBody] UserSetPasswordDto passwordDto)
    {
        try
        {
            _logger.LogTrace("User {id} set password at {dateTime}.", id, DateTime.Now);

            var kresApi = await _service.Users.SetPasswordAsync(id, passwordDto?.NewPassword);
            if (!kresApi.IsValid)
                return BadRequest(kresApi);

            _passwordStamps.Forget(kresApi.Entity.UserName);
            return Ok(kresApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User {id} set password at {dateTime}.", id, DateTime.Now);
            var kresApi = new Result<UserDto>();
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    #region Forgotten password (anonymous)

    // Sends a link to set a new password to the email of the user with that user name or email. The answer is the
    // same whether there is such a user or not, so it doesn't tell who is registered.
    [AllowAnonymous]
    [EnableRateLimiting(KntRateLimits.PasswordReset)]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] UserForgotPasswordDto request)
    {
        var kresApi = new Result<string>();
        try
        {
            if (!_passwordResetMail.IsAvailable)
            {
                kresApi.AddErrorMessage("Setting a new password by email is not available in this server. Ask an administrator to set it.");
                return BadRequest(kresApi);
            }

            var kresService = await _service.Users.CreatePasswordResetAsync(request?.UserNameOrEMail, _appSettings.Secret, _passwordResetMail.LinkLifetime);
            if (kresService.IsValid)
                _passwordResetMail.SendInBackground(kresService.Entity);
            else
                _logger.LogInformation("Password reset asked for {user}, without a user to send it to.", request?.UserNameOrEMail);

            kresApi.Entity = "If there is a user with that name or email, a message with a link to set a new password has been sent to its email.";
            return Ok(kresApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password reset asked for {user} at {dateTime}.", request?.UserNameOrEMail, DateTime.Now);
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    // Sets a new password with the token of that link, and signs the user in (a new token, as with ChangePassword).
    [AllowAnonymous]
    [EnableRateLimiting(KntRateLimits.PasswordReset)]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] UserResetPasswordDto reset)
    {
        var kresApi = new Result<UserTokenDto>();
        try
        {
            var kresService = await _service.Users.ResetPasswordAsync(reset, _appSettings.Secret);
            if (!kresService.IsValid)
            {
                kresApi.AddListErrorMessage(kresService.ListErrorMessage);
                return BadRequest(kresApi);
            }

            _logger.LogInformation("User {user} set a new password with a reset link.", kresService.Entity.UserName);
            kresApi.Entity = await BuildTokenAsync(kresService.Entity);
            return Ok(kresApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password reset at {dateTime}.", DateTime.Now);
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    #endregion

    #region The signed in user's own account (any role)

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var kresApi = await _service.Users.GetProfileAsync();
            return kresApi.IsValid ? Ok(kresApi) : BadRequest(kresApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User {user} get profile at {dateTime}.", _service.UserIdentityName, DateTime.Now);
            var kresApi = new Result<UserProfileDto>();
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    [HttpPut("me")]
    public async Task<IActionResult> SaveProfile([FromBody] UserProfileDto profile)
    {
        try
        {
            _logger.LogTrace("User {user} save profile at {dateTime}.", _service.UserIdentityName, DateTime.Now);

            var kresApi = await _service.Users.SaveProfileAsync(profile);
            return kresApi.IsValid ? Ok(kresApi) : BadRequest(kresApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User {user} save profile at {dateTime}.", _service.UserIdentityName, DateTime.Now);
            var kresApi = new Result<UserProfileDto>();
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    // The password changes and the user's other sessions are closed: the answer is a new token for this one.
    [HttpPost("me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] UserChangePasswordDto passwords)
    {
        var kresApi = new Result<UserTokenDto>();
        try
        {
            _logger.LogTrace("User {user} change password at {dateTime}.", _service.UserIdentityName, DateTime.Now);

            var kresService = await _service.Users.ChangePasswordAsync(passwords);
            if (!kresService.IsValid)
            {
                kresApi.AddListErrorMessage(kresService.ListErrorMessage);
                return BadRequest(kresApi);
            }

            kresApi.Entity = await BuildTokenAsync(kresService.Entity);
            return Ok(kresApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User {user} change password at {dateTime}.", _service.UserIdentityName, DateTime.Now);
            kresApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(kresApi);
        }
    }

    #endregion

    #region Private methods

    private async Task<UserTokenDto> BuildTokenAsync(UserDto userDto)
    {
        var passwordStamp = await _passwordStamps.RefreshAsync(_service, userDto.UserName);

        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name, userDto.UserName),
            new Claim(JwtRegisteredClaimNames.UniqueName, userDto.EMail),
            new Claim(KntPasswordStamps.ClaimType, passwordStamp ?? ""),
                            
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
