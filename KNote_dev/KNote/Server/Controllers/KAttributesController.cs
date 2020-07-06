using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KNote.Server.Helpers;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;


namespace KNote.Server.Controllers
{    
    //[Authorize(Roles = "Administrators")]
    [ApiController]
    [Route("api/[controller]")]
    public class KAttributesController : ControllerBase
    {
        private IKntService _service { get; set; }
        private readonly AppSettings _appSettings;

        public KAttributesController(IKntService service, IOptions<AppSettings> appSettings)
        {
            _service = service;
            _appSettings = appSettings.Value;

        }

        [HttpGet]    // GET api/kattributes       
        public async Task<IActionResult> Get()
        {
            try
            {
                var kresApi = await _service.KAttributes.GetAllAsync();
                if (kresApi.IsValid)
                    return Ok(kresApi);
                else
                    return BadRequest(kresApi);
            }
            catch (Exception ex)
            {
                var kresApi = new Result<List<NoteTypeDto>>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [HttpGet("[action]/{typeId}")]    // GET api/kattributes/getfornotetype/typeId      
        public async Task<IActionResult> GetForNoteType(Guid? typeId)
        {
            try
            {
                var kresApi = await _service.KAttributes.GetAllAsync(typeId);
                if (kresApi.IsValid)
                    return Ok(kresApi);
                else
                    return BadRequest(kresApi);
            }
            catch (Exception ex)
            {
                var kresApi = new Result<List<NoteTypeDto>>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [HttpGet("{id}")]    // GET api/kattributes/guidKAttribute
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var resApi = await _service.KAttributes.GetAsync(id);
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


        [HttpPost]   // POST api/kattributes
        [HttpPut]    // PUT api/kattributes
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Post([FromBody]KAttributeDto entity)
        {
            try
            {
                var kresApi = await _service.KAttributes.SaveAsync(entity);
                if (kresApi.IsValid)
                    return Ok(kresApi);
                else
                    return BadRequest(kresApi);
            }
            catch (Exception ex)
            {
                var kresApi = new Result<KAttributeInfoDto>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [HttpDelete("{id}")]    // DELETE api/kattributes/guid        
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var resApi = await _service.KAttributes.DeleteAsync(id);
                if (resApi.IsValid)
                    return Ok(resApi);
                else
                    return BadRequest(resApi);
            }
            catch (Exception ex)
            {
                var resApi = new Result<NoteTypeDto>();
                resApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(resApi);
            }
        }

        [HttpDelete("[action]/{id}")]    // DELETE api/kattributes/deleterabulatedvalue/guid        
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTabulatedValue(Guid id)
        {
            try
            {
                var resApi = await _service.KAttributes.DeleteKAttributeTabulatedValueAsync(id);
                if (resApi.IsValid)
                    return Ok(resApi);
                else
                    return BadRequest(resApi);
            }
            catch (Exception ex)
            {
                var resApi = new Result<NoteTypeDto>();
                resApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(resApi);
            }
        }

        [HttpGet("[action]/{idAttribute}")]    // GET api/kattributes/getattributetabulatedvalues/guid 
        public async Task<IActionResult> GetAttributeTabulatedValues(Guid idAttribute)
        {
            try
            {
                var resApi = await _service.KAttributes.GetKAttributeTabulatedValuesAsync(idAttribute);
                if (resApi.IsValid)
                    return Ok(resApi);
                else
                    return BadRequest(resApi);
            }
            catch (Exception ex)
            {
                var resApi = new Result<NoteTypeDto>();
                resApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(resApi);
            }
        }

    }

}
