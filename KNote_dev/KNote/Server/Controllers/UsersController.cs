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

using KNote.DomainModel.Services;
using KNote.Server.Helpers;
using KNote.Shared;
using KNote.Shared.Dto;


namespace KNote.Server.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private IKntService _service { get; set; }
        private readonly AppSettings _appSettings;

        // TODO: !!! Esta propiedad estaba en la versión antiga. 
        //       en la versión actual.
        //public object SecurityAlgorithms { get; private set; }

        public UsersController(IKntService service, IOptions<AppSettings> appSettings)
        {
            _service = service;
            _appSettings = appSettings.Value;
        }

        //[HttpGet]    // GET api/users        
        //public IActionResult Get()
        //{
        //    try
        //    {
        //        var kresApi = _service.Users.GetAll();
        //        if (kresApi.IsValid)
        //            return Ok(kresApi);
        //        else
        //            return BadRequest(kresApi);
        //    }
        //    catch (Exception ex)
        //    {
        //        var kresApi = new Result<List<UserInfoDto>>();
        //        kresApi.AddErrorMessage("Generic error: " + ex.Message);
        //        return BadRequest(kresApi);
        //    }
        //}

        [HttpGet]    // GET api/users        
        public async Task<IActionResult> Get([FromQuery] PaginationDto pagination)
        {
            try
            {
                var count = (await _service.Users.GetCount()).Entity;
                HttpContext.InsertPaginationParamInResponse(count, pagination.NumRecords);

                var kresApi = await _service.Users.GetAllAsync(pagination);

                if (kresApi.IsValid)
                    return Ok(kresApi);
                else
                    return BadRequest(kresApi);
            }
            catch (Exception ex)
            {
                var kresApi = new Result<List<UserInfoDto>>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [HttpGet("[action]/{userName}")]    // GET api/users/afumfer2        
        public IActionResult GetByName(string userName)
        {
            try
            {
                var kresApi = _service.Users.Get(userName);
                if (kresApi.IsValid)
                    return Ok(kresApi);
                else
                    return BadRequest(kresApi);

            }
            catch (Exception ex)
            {
                var kresApi = new Result<List<UserInfoDto>>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [HttpGet("{id}")]    // GET api/kattributes/guidKAttribute
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
                var kresApi = new Result<KAttributeInfoDto>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }


        [HttpGet("[action]/{userName}")]    // GET api/users/GetMessagesTo/afumfer2        
        public IActionResult GetMessages(Guid id)
        {
            try
            {
                var kresApi = _service.Users.GetMessages(id);
                if (kresApi.IsValid)
                    return Ok(kresApi);
                else
                    return BadRequest(kresApi);
            }
            catch (Exception ex)
            {
                var kresApi = new Result<List<UserInfoDto>>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [HttpPost]   // POST api/users
        [HttpPut]    // PUT api/users
        public async Task<IActionResult> Post([FromBody]UserDto userDto)
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

        [HttpDelete("{id}")]    // DELETE api/users/guid
        //[Authorize(Roles = "Administrators")]
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
                var kresApi = new Result<List<UserInfoDto>>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]UserRegisterDto user)
        {
            try
            {
                var kresApi = _service.Users.Create(user);
                if (kresApi.IsValid)
                    return Ok(kresApi);
                else
                    return BadRequest(kresApi);
            }
            catch (Exception ex)
            {
                var kresApi = new Result<List<UserInfoDto>>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody]UserCredentialsDto credentials)
        {
            try
            {
                var kres = _service.Users.Authenticate(credentials.Name, credentials.Password);
                var user = kres.Entity;

                if (!kres.IsValid)
                {
                    return Ok(new UserTokenDto { success = false, token = "" });
                    //return BadRequest(kresApi);
                }

                // TODO: !!! Esto es código viejo eliminar ....
                //var tokenHandler = new JwtSecurityTokenHandler();
                //var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                //var tokenDescriptor = new SecurityTokenDescriptor
                //{
                //    Subject = new ClaimsIdentity(new Claim[]
                //    {                        
                //        new Claim(JwtRegisteredClaimNames.UniqueName, user.EMail),
                //        new Claim(ClaimTypes.Name, user.EMail),
                //        new Claim(ClaimTypes.Role, user.RoleDefinition),
                //        new Claim("KNTapp", "KNoteWeb" ),
                //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

                //        // --------

                //    }),
                //    Expires = DateTime.UtcNow.AddDays(365),
                //    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)                    
                //};
                //var token = tokenHandler.CreateToken(tokenDescriptor);
                //var tokenString = tokenHandler.WriteToken(token);

                //return Ok(new UserTokenDto { success = true, token = tokenString, uid = user.UserId.ToString() });

                return Ok(BuildToken(user));
            }
            catch
            {
                return Ok(new UserTokenDto { success = false, token = "", uid = "" });
            }
        }

        private UserTokenDto BuildToken(UserDto userDto)
        {
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, userDto.EMail),
                new Claim(ClaimTypes.Name, userDto.UserName),
                //new Claim("miValor", "Lo que yo quiera"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };


            // TODO: Pendiente de transformar la cadena de roles un array de Claims
            //foreach (var rol in roles)
            //{
            //    claims.Add(new Claim(ClaimTypes.Role, rol));
            //}
            claims.Add(new Claim(ClaimTypes.Role, userDto.RoleDefinition));

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



    }
}
