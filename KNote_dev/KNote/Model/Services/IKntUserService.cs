using KNote.Model;
using KNote.Model.Dto;
using KNote.Model.Dto.Info;
using KNote.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Services
{
    public interface IKntUserService
    {        
        Task<Result<List<UserInfoDto>>> GetAllAsync(PaginationDto pagination = null);
        Task<Result<UserDto>> GetAsync(Guid userId);   
        Task<Result<UserDto>> SaveAsync(UserDto entityInfo);
        Task<Result<UserInfoDto>> DeleteAsync(Guid userId);
        Task<Result<int>> GetCount();
        Result<UserDto> Authenticate(string username, string password);
        Result<UserDto> Create(UserRegisterDto userRegisterInfoDto);
    }
}
