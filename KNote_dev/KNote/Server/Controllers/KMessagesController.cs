using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KNote.DomainModel.Services;
using KNote.Server.Helpers;
using KNote.Shared;
using KNote.Shared.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KNote.Server.Controllers
{
    // [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class KMessages : ControllerBase
    {
        private IKntService _service { get; set; }
        private readonly AppSettings _appSettings;

        public KMessages(IKntService service, IOptions<AppSettings> appSettings)
        {
            _service = service;
            _appSettings = appSettings.Value;
        }


        // TODO: Eliminar este método. (Sólo para periodo de pruebas)
        [HttpGet]   // GET api/kmessages
        public IActionResult Get()
        {
            var kresApi = new Result<List<KMessageInfoDto>>();
            try
            {
                kresApi = _service.KMessages.GetAll();
                if (kresApi.IsValid)
                    return Ok(kresApi);
                else
                    return BadRequest(kresApi);
            }
            catch (Exception ex)
            {
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [HttpGet("{messageId}")]    // GET api/kmessages/guidmessage
        public IActionResult Get(Guid messageId)
        {
            var kresApi = new Result<KMessageInfoDto>();
            try
            {
                kresApi = _service.KMessages.Get(messageId);
                if (kresApi.IsValid)
                    return Ok(kresApi);
                else
                    return BadRequest(kresApi);
            }
            catch (Exception ex)
            {
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }


        [HttpGet("[action]/{noteId}")]    // GET api/kmessages/GetToNote/xx        
        public IActionResult GetToNote(Guid noteId)
        {
            var kresApi = new Result<List<KMessageInfoDto>>();
            try
            {
                kresApi = _service.KMessages.GetAllForNote(noteId);
                if (kresApi.IsValid)
                    return Ok(kresApi);
                else
                    return BadRequest(kresApi);
            }
            catch (Exception ex)
            {
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }


        [HttpGet("[action]/{userId}")]    // GET api/kmessages/GetToUser/xx        
        public IActionResult GetToUser(Guid userId)
        {
            var kresApi = new Result<List<KMessageInfoDto>>();
            try
            {
                kresApi = _service.KMessages.GetAllForUser(userId);
                if (kresApi.IsValid)
                    return Ok(kresApi);
                else
                    return BadRequest(kresApi);
            }
            catch (Exception ex)
            {
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }


        [HttpPost]   // POST api/kmessages
        [HttpPut]    // PUT api/kmessages
        public async Task<IActionResult> Post([FromBody]KMessageInfoDto kmessageInfo)
        {
            var kresApi = new Result<KMessageInfoDto>();
            try
            {
                kresApi = await _service.KMessages.SaveAsync(kmessageInfo);
                if (kresApi.IsValid)
                    return Ok(kresApi);
                else
                    return BadRequest(kresApi);
            }
            catch (Exception ex)
            {
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }


        [HttpDelete("{id}")]    // DELETE api/kmessages/guid
        //[Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var kresApi = new Result<KMessageInfoDto>();
            try
            {
                kresApi = await _service.KMessages.DeleteAsync(id);
                if (kresApi.IsValid)
                    return Ok(kresApi);
                else
                    return BadRequest(kresApi);
            }
            catch (Exception ex)
            {
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }
    }

}
