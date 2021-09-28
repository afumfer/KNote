using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using KNote.Service;
using KNote.Server.Helpers;
using KNote.Model;
using KNote.Model.Dto;
using System.Threading;

namespace KNote.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private IKntService _service { get; set; }
        private readonly AppSettings _appSettings;


        public UsersController(IKntService service, IOptions<AppSettings> appSettings)
        {
            _service = service;
            _appSettings = appSettings.Value;
        }

        //[HttpGet("[action]")]    // GET api/users/getall        
        //public async Task<IActionResult> GetAll()
        //{
        //    try
        //    {
        //        var kresApi = await _service.Users.GetAllAsync();
        //        if (kresApi.IsValid)
        //            return Ok(kresApi);
        //        else
        //            return BadRequest(kresApi);
        //    }
        //    catch (Exception ex)
        //    {
        //        var kresApi = new Result<List<UserDto>>();
        //        kresApi.AddErrorMessage("Generic error: " + ex.Message);
        //        return BadRequest(kresApi);
        //    }
        //}

        [HttpGet]    // GET api/users        
        public async Task<IActionResult> Get([FromQuery] PageIdentifier pagination)
        {
            try
            {
                pagination.PageNumber = (pagination.PageNumber < 1) ? 1 : pagination.PageNumber;
                pagination.PageSize = (pagination.PageSize < 1) ? 9999 : pagination.PageSize;

                var kresApi = await _service.Users.GetAllAsync(pagination);

                if (kresApi.IsValid)
                    return Ok(kresApi);
                else
                    return BadRequest(kresApi);
            }
            catch (Exception ex)
            {
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
                var kresApi = await _service.Users.SaveAsync(userDto);
                if (kresApi.IsValid)
                    return Ok(kresApi);
                else
                    return BadRequest(kresApi);
            }
            catch (Exception ex)
            {
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
                var kresApi = await _service.Users.DeleteAsync(id);
                if (kresApi.IsValid)
                    return Ok(kresApi);
                else
                    return BadRequest(kresApi);
            }
            catch (Exception ex)
            {
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
                var kresRep = await _service.Users.Create(user);
                if (kresRep.IsValid)                    
                    return Ok(BuildToken(kresRep.Entity));
                else                    
                    return BadRequest(new UserTokenDto { success = kresRep.IsValid, token = "", error = kresRep.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new UserTokenDto { success = false, token = "", error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody]UserCredentialsDto credentials)
        {            
            try
            {
                var kresRep = await _service.Users.Authenticate(credentials.Name, credentials.Password);
                var user = kresRep.Entity;

                if (!kresRep.IsValid)
                {                    
                    return BadRequest(new UserTokenDto { success = false, token = "", error = kresRep.Message });
                }

                return Ok(BuildToken(user));
            }
            catch (Exception ex)
            {
                return BadRequest(new UserTokenDto { success = false, token = "", uid = "", error = ex.Message });
            }
        }

        #region Private methods

        private UserTokenDto BuildToken(UserDto userDto)
        {
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, userDto.EMail),
                new Claim(ClaimTypes.Name, userDto.UserName),
                //new Claim("KaNoteApp", "KaNoteWeb"),
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

        [HttpGet]
        [Route("[action]")]
        public IActionResult EchoPing()
        {
            return Ok(true);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult EchoUser()
        {
            var identity = Thread.CurrentPrincipal.Identity;
            return Ok($" IPrincipal-user: {identity.Name} - IsAuthenticated: {identity.IsAuthenticated}");
        }

        #endregion 

    }
}
