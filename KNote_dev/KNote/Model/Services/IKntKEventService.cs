﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;
using KNote.Model.Entities;

namespace KNote.Model.Services
{
    public interface IKntKEventService
    {
        Task<Result<List<KEventDto>>> GetAllAsync();
        Task<Result<KEventDto>> GetAsync(Guid eventId);        
        Task<Result<KEventDto>> SaveAsync(KEventDto entityInfo);
        Task<Result<KEventDto>> DeleteAsync(Guid id);
    }
}
