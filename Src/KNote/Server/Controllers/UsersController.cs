using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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
    private readonly KntRepositories _repositories;
    private readonly KntRepositoryEntry _repository;
    private readonly AppSettings _appSettings;
    private readonly KntPasswordStamps _passwordStamps;
    private readonly KntPasswordResetMail _passwordResetMail;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IKntService service, KntRepositories repositories, IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> appSettings,
        KntPasswordStamps passwordStamps, KntPasswordResetMail passwordResetMail, ILogger<UsersController> logger)
    {
        _service = service;
        _service.Logger = logger;
        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
        // The repository of _service: the session's, or the default one without a user.
        _repositories = repositories;
        _repository = repositories.ForUser(httpContextAccessor.HttpContext.User);
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

            // In the default repository. The new user's role is decided by the service (KntUsersRegisterAsyncCommand),
            // not by the RoleDefinition this anonymous endpoint receives.
            using var service = NewService(_repositories.Default);
            var kresService = await service.Users.RegisterAsync(user);

            if (kresService.IsValid)
            {
                return Ok(await BuildTokenAsync(kresService.Entity, _repositories.Default, service));
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

    // A session is in one repository. With the credentials only, they are checked in every repository: valid in one,
    // its token; valid in several, those repositories (and no token), to choose one and sign in again with it
    // (credentials.RepositoryId). Nothing tells an anonymous caller which repositories there are.
    [AllowAnonymous]
    [EnableRateLimiting(KntRateLimits.SignIn)]
    [HttpPost("[action]")]
    public async Task<IActionResult> Login([FromBody]UserCredentialsDto credentials)
    {
        try
        {
            _logger.LogTrace("User {user} Login at {dateTime}.", credentials.UserName, DateTime.Now);

            IEnumerable<KntRepositoryEntry> candidates = credentials.RepositoryId is Guid repositoryId
                ? new[] { _repositories.Find(repositoryId) }.Where(r => r != null)
                : _repositories.All;

            var valid = new List<(KntRepositoryEntry Repository, UserDto User, string Name)>();
            string error = "User not authenticated";
            foreach (var repository in candidates)
            {
                using var service = NewService(repository);
                var kresRep = await service.Users.AuthenticateAsync(credentials);
                if (kresRep.IsValid)
                    valid.Add((repository, kresRep.Entity, (await service.SystemValues.GetRepositoryInfoAsync()).Entity?.Name));
                else
                    error = kresRep.ErrorMessage;
            }

            if (valid.Count == 0)
            {
                _logger.LogWarning("User {user} login BadRequest at {dateTime}.", credentials.UserName, DateTime.Now);
                return BadRequest(new UserTokenDto { success = false, token = "", error = error });
            }

            if (valid.Count > 1)
                return Ok(new UserTokenDto
                {
                    success = false,
                    token = "",
                    error = "Your user has access to several repositories in this server: choose the one to sign in to.",
                    Repositories = valid.Select(v => new RepositoryInfoDto { RepositoryId = v.Repository.RepositoryId, Name = v.Name }).ToList()
                });

            using var signInService = NewService(valid[0].Repository);
            return Ok(await BuildTokenAsync(valid[0].User, valid[0].Repository, signInService));
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

            _passwordStamps.Forget(_repository.RepositoryId, kresApi.Entity.UserName);
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

    // Sends a link to set a new password to the email of the user with that user name or email: one link for every
    // repository where that user is with the same user name and email (the same person: the mailbox is what proves who
    // asks, so a user of another repository with another email gets their own link at their own address). The answer
    // is the same whether there is such a user or not, so it doesn't tell who is registered.
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

            var tokens = new List<KntRepositoryResetToken>();
            foreach (var repository in _repositories.All)
            {
                using var service = NewService(repository);
                var kresService = await service.Users.CreatePasswordResetAsync(request?.UserNameOrEMail, _appSettings.Secret, _passwordResetMail.LinkLifetime);
                if (kresService.IsValid)
                    tokens.Add(new KntRepositoryResetToken(repository.RepositoryId, (await service.SystemValues.GetRepositoryInfoAsync()).Entity?.Name, kresService.Entity));
            }

            foreach (var tokensOfAUser in tokens.GroupBy(t => $"{t.Reset.UserName.Trim()}\n{t.Reset.EMail.Trim()}", StringComparer.OrdinalIgnoreCase))
                _passwordResetMail.SendInBackground(tokensOfAUser.ToList());
            if (tokens.Count == 0)
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

    // Sets a new password with the token of that link in each of its repositories (Service checks each token in its
    // own). Changed in one, the user is signed in to it (a new token, as with ChangePassword); in several, the answer
    // is them (Repositories) and the user's name, to sign in as with the sign in's second step. Notice tells where it
    // couldn't be set (e.g. the password changed there meanwhile): asking for another link sets it there.
    [AllowAnonymous]
    [EnableRateLimiting(KntRateLimits.PasswordReset)]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] UserResetPasswordDto reset)
    {
        var kresApi = new Result<UserTokenDto>();
        try
        {
            var tokens = KntPasswordResetLinkToken.Read(reset?.Token);
            if (tokens == null)
            {
                kresApi.AddErrorMessage("The link to set a new password is not valid or has expired. Ask for a new one.");
                return BadRequest(kresApi);
            }

            var changed = new List<(KntRepositoryEntry Repository, UserDto User, string Name)>();
            var failed = new List<(string Name, Result<UserDto> Result)>();
            foreach (var (repositoryId, token) in tokens)
            {
                var repository = _repositories.Find(repositoryId);
                if (repository == null)
                    continue;

                using var service = NewService(repository);
                var name = (await service.SystemValues.GetRepositoryInfoAsync()).Entity?.Name;
                var kresService = await service.Users.ResetPasswordAsync(new UserResetPasswordDto { Token = token, NewPassword = reset.NewPassword }, _appSettings.Secret);
                if (kresService.IsValid)
                {
                    _passwordStamps.Forget(repositoryId, kresService.Entity.UserName);
                    changed.Add((repository, kresService.Entity, name));
                }
                else
                    failed.Add((name, kresService));
            }

            if (changed.Count == 0)
            {
                // The same reason in every repository (the same user, so the same password policy), or no repository.
                if (failed.Count > 0)
                    kresApi.AddListErrorMessage(failed[0].Result.ListErrorMessage);
                else
                    kresApi.AddErrorMessage("The link to set a new password is not valid or has expired. Ask for a new one.");
                return BadRequest(kresApi);
            }

            var user = changed[0].User;
            _logger.LogInformation("User {user} set a new password with a reset link in {count} repositories.", user.UserName, changed.Count);
            if (changed.Count == 1)
            {
                using var service = NewService(changed[0].Repository);
                kresApi.Entity = await BuildTokenAsync(user, changed[0].Repository, service);
            }
            else
                kresApi.Entity = new UserTokenDto
                {
                    success = true,
                    token = "",
                    UserName = user.UserName,
                    Repositories = changed.Select(c => new RepositoryInfoDto { RepositoryId = c.Repository.RepositoryId, Name = c.Name }).ToList()
                };
            if (failed.Count > 0)
                kresApi.Entity.Notice = $"The new password is set in: {string.Join(", ", changed.Select(c => c.Name))}. " +
                    $"It couldn't be set in: {string.Join("; ", failed.Select(f => $"{f.Name}: {f.Result.ErrorMessage}"))}.";
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

    // The password changes and the user's other sessions are closed: the answer is a new token for this one. As in
    // ClientWin, it changes in the session's repository and then in the others where the current password signs this
    // user in (the same credentials, so the same person); the answer's Notice tells where it changed.
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

            var changed = new List<string> { (await _service.SystemValues.GetRepositoryInfoAsync()).Entity?.Name };
            var failed = new List<string>();
            foreach (var repository in _repositories.All.Where(r => r.RepositoryId != _repository.RepositoryId))
            {
                using var service = NewService(repository);
                var signedIn = await service.Users.AuthenticateAsync(new UserCredentialsDto { UserName = _service.UserIdentityName, Password = passwords?.CurrentPassword });
                if (!signedIn.IsValid)
                    continue;

                service.UserIdentityName = _service.UserIdentityName;
                var name = (await service.SystemValues.GetRepositoryInfoAsync()).Entity?.Name;
                var res = await service.Users.ChangePasswordAsync(passwords);
                if (res.IsValid)
                {
                    _passwordStamps.Forget(repository.RepositoryId, _service.UserIdentityName);
                    changed.Add(name);
                }
                else
                    failed.Add($"{name}: {res.ErrorMessage}");
            }

            kresApi.Entity = await BuildTokenAsync(kresService.Entity);
            if (changed.Count > 1 || failed.Count > 0)
                kresApi.Entity.Notice = $"The password changed in: {string.Join(", ", changed)}."
                    + (failed.Count > 0 ? $" It couldn't be changed in: {string.Join("; ", failed)}." : "");
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

    // A service of another repository than the request's (the caller disposes it), without a user.
    private IKntService NewService(KntRepositoryEntry repository)
    {
        var service = _repositories.CreateService(repository, null, Request);
        service.Logger = _logger;
        return service;
    }

    // A token for the user in a repository (the session's repository), with the user's roles and password fingerprint
    // there; service is a service of that repository.
    private Task<UserTokenDto> BuildTokenAsync(UserDto userDto) => BuildTokenAsync(userDto, _repository, _service);

    private async Task<UserTokenDto> BuildTokenAsync(UserDto userDto, KntRepositoryEntry repository, IKntService service)
    {
        var passwordStamp = await _passwordStamps.RefreshAsync(repository.RepositoryId, service, userDto.UserName);

        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name, userDto.UserName),
            new Claim(JwtRegisteredClaimNames.UniqueName, userDto.EMail),
            new Claim(KntPasswordStamps.ClaimType, passwordStamp ?? ""),
            new Claim(KntConst.RepositoryClaimType, repository.RepositoryId.ToString()),
                            
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
