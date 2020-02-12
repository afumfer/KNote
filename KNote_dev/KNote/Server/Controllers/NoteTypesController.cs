﻿using System;
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

        [HttpGet]    // GET api/notetypes       
        public IActionResult Get()
        {
            try
            {
                var kresApi = _service.NoteTypes.GetAll();
                if (kresApi.IsValid)
                    return Ok(kresApi);
                else
                    return BadRequest(kresApi);
            }
            catch (Exception ex)
            {
                var kresApi = new Result<List<NoteTypeInfoDto>>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

        [HttpGet("{id}")]    // GET api/notetypes/guidnotetype
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var resApi = await _service.NoteTypes.GetAsync(id);
                if (resApi.IsValid)
                    return Ok(resApi);
                else
                {
                    return BadRequest(resApi);
                }
            }
            catch (Exception ex)
            {
                var kresApi = new Result<NoteTypeInfoDto>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }


        [HttpPost]   // POST api/notetypes
        [HttpPut]    // PUT api/notetypes
        public async Task<IActionResult> Post([FromBody]NoteTypeDto noteType)
        {
            try
            {                                
                var kresApi = await _service.NoteTypes.SaveAsync(noteType);
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

        [HttpDelete("{id}")]    // DELETE api/notetypes/guid        
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var resApi = await _service.NoteTypes.DeleteAsync(id);
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