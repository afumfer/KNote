using KNote.Model;
using KNote.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNote.Client.ClientDataServices
{
    public interface IUserWebApiService
    {        
        Task<Result<List<UserDto>>> GetAllAsync(PageIdentifier pagination = null);
        Task<Result<UserDto>> DeleteAsync(Guid userId);

        //Task<Result<UserDto>> GetAsync(Guid userId);
        //Task<Result<UserDto>> SaveAsync(UserDto entityInfo);

        //Task<Result<UserDto>> GetByUserNameAsync(string userName);
        //Task<Result<UserDto>> Authenticate(string username, string password);
        //Task<Result<UserDto>> Create(UserRegisterDto userRegisterInfoDto);
    }
}
