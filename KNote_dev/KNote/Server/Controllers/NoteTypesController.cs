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
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NoteTypesController : ControllerBase
    {
        private IKntService _service { get; set; }
        private readonly AppSettings _appSettings;

        public NoteTypesController(IKntService service, IOptions<AppSettings> appSettings)
        {
            _service = service;
            _appSettings = appSettings.Value;
        }

        [HttpPost]   // POST api/notetypes
        [HttpPut]    // PUT api/notetypes
        public async Task<IActionResult> Post([FromBody]NoteTypeDto noteType)
        {
            try
            {
                // TODO: Implementar _service.NoteTypes.xxxxx
                var kresApi = new Result<NoteTypeDto>(); // await _service.N.SaveAsync(noteType);
                if (kresApi.IsValid)
                    return Ok(kresApi);
                else
                    return BadRequest(kresApi);
            }
            catch (Exception ex)
            {
                var kresApi = new Result<NoteTypeDto>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

    }
}
