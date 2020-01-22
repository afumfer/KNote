using KNote.Shared;
// TODO: Pendiente de eliminar
//using KNote.DomainModel.Dto;
using KNote.Shared.Dto;
using KNote.DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KNote.DomainModel.Services
{
    public interface IKntUserService
    {
        Result<List<UserInfoDto>> GetAll();
        Result<UserInfoDto> Get(string userName);
        Result<UserInfoDto> Get(Guid userId);        
        Result<UserDto> GetMessages(Guid id);
        Result<UserDto> New(UserInfoDto entity = null);
        Result<UserInfoDto> Save(UserInfoDto entity);
        Task<Result<UserInfoDto>> SaveAsync(UserInfoDto entityInfo);        
        Result<UserInfoDto> Authenticate(string username, string password);
        Result<UserInfoDto> Create(UserRegisterInfoDto userRegisterInfoDto);
        Task<Result<UserInfoDto>> DeleteAsync(Guid userId);
        Result<UserInfoDto> Delete(Guid userId);
    }
}
