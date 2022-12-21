using KNote.Model;
using KNote.Model.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KNote.Service.Interfaces
{
    public interface IKntUserService
    {
        Task<Result<List<UserDto>>> GetAllAsync(PageIdentifier pagination = null);
        Task<Result<UserDto>> GetAsync(Guid userId);
        Task<Result<UserDto>> GetByUserNameAsync(string userName);
        Task<Result<UserDto>> SaveAsync(UserDto user);
        Task<Result<UserDto>> DeleteAsync(Guid userId);
        Task<Result<UserDto>> AuthenticateAsync(UserCredentialsDto userCredentials);
        Task<Result<UserDto>> CreateAsync(UserRegisterDto userRegisterInfoDto);
    }
}
