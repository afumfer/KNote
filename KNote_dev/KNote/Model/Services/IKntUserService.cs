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
        Result<List<UserInfoDto>> GetAll();
        Result<UserDto> Get(string userName);
        Task<Result<UserDto>> GetAsync(Guid userId);
        Result<UserDto> GetMessages(Guid id);
        Result<UserDto> New(UserInfoDto entity = null);
        Result<UserDto> Save(UserDto entity);
        Task<Result<UserDto>> SaveAsync(UserDto entityInfo);        
        Result<UserDto> Authenticate(string username, string password);
        Result<UserDto> Create(UserRegisterDto userRegisterInfoDto);
        Task<Result<UserInfoDto>> DeleteAsync(Guid userId);
        Result<UserInfoDto> Delete(Guid userId);
        Task<Result<int>> GetCount();
        Task<Result<List<UserInfoDto>>> GetAllAsync(PaginationDto pagination);
    }
}
