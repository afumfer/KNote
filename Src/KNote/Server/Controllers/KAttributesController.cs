﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.Logging;
using KNote.Client.Shared;

namespace KNote.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class KAttributesController : ControllerBase
{
    private readonly IKntService _service;
    private readonly ILogger<KAttributesController> _logger;

    public KAttributesController(IKntService service, IHttpContextAccessor httpContextAccessor, ILogger<KAttributesController> logger)
    {
        _service = service;
        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
        _logger = logger;
    }

    [HttpGet]    // GET api/kattributes       
    public async Task<IActionResult> Get()
    {
        try
        {
            _logger.LogTrace("Get at {dateTime}.", DateTime.Now);

            var kresApi = await _service.KAttributes.GetAllAsync();
            if (kresApi.IsValid)
                return Ok(kresApi);
            else
                return BadRequest(kresApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get at {dateTime}.", DateTime.Now);
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
            _logger.LogTrace("GetForNoteType {id} get at {dateTime}.", typeId, DateTime.Now);

            var kresApi = await _service.KAttributes.GetAllAsync(typeId);
            if (kresApi.IsValid)
                return Ok(kresApi);
            else
                return BadRequest(kresApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetForNoteType at {dateTime}.", DateTime.Now);
            var kresApi = new Result<List<NoteTypeDto>>();
            kresApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(kresApi);
        }
    }

    [HttpGet("{id}")]    // GET api/kattributes/id
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            _logger.LogTrace("Get {id} get at {dateTime}.", id, DateTime.Now);

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
            _logger.LogError(ex, "Get {id} get at {dateTime}", id, DateTime.Now);
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
            _logger.LogTrace("Post {name} get at {dateTime}.", entity.Name?.ToString(), DateTime.Now);

            var kresApi = await _service.KAttributes.SaveAsync(entity);
            if (kresApi.IsValid)
                return Ok(kresApi);
            else
                return BadRequest(kresApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Post {name} get at {dateTime}", entity.Name?.ToString(), DateTime.Now);
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
            _logger.LogTrace("Delete {id} get at {dateTime}.", id, DateTime.Now);

            var resApi = await _service.KAttributes.DeleteAsync(id);
            if (resApi.IsValid)
                return Ok(resApi);
            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete {id} delete at {dateTime}.", id, DateTime.Now);
            var resApi = new Result<NoteTypeDto>();
            resApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(resApi);
        }
    }
    
    [HttpGet("{idAttribute}/[action]")]    // GET api/kattributes/guid/getattributetabulatedvalues
    public async Task<IActionResult> GetTabulatedValues(Guid idAttribute)
    {
        try
        {
            _logger.LogTrace("GetTabulatedValues {id} get at {dateTime}.", idAttribute, DateTime.Now);

            var resApi = await _service.KAttributes.GetKAttributeTabulatedValuesAsync(idAttribute);
            if (resApi.IsValid)
                return Ok(resApi);
            else
                return BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTabulatedValues {id} delete at {dateTime}.", idAttribute, DateTime.Now);
            var resApi = new Result<NoteTypeDto>();
            resApi.AddErrorMessage("Generic error: " + ex.Message);
            return BadRequest(resApi);
        }
    }

}
