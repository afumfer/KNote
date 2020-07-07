using KNote.Model;
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
        Task<Result<UserDto>> SaveAsync(UserDto entityInfo);
        Task<Result<UserDto>> DeleteAsync(Guid userId);
        Task<Result<int>> GetCount();
    }
}
