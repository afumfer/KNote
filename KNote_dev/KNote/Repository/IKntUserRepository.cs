﻿using KNote.Model;
using KNote.Model.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KNote.Repository
{
    public interface IKntUserRepository : IDisposable
    {
        Task<Result<List<UserDto>>> GetAllAsync(PaginationDto pagination = null);
        Task<Result<UserDto>> GetAsync(Guid userId);
        Task<Result<UserInternalDto>> GetInternalAsync(string userName);        
        Task<Result<UserDto>> AddAsync(UserDto userEntity);
        Task<Result<UserDto>> UpdateAsync(UserDto userEntity);
        Task<Result<UserInternalDto>> AddInternalAsync(UserInternalDto userEntity);
        Task<Result> DeleteAsync(Guid userId);
        Task<Result<int>> GetCount();
    }
}
