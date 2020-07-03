using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.ServiceEF.Repositories;
using KNote.Model;
using KNote.Model.Entities;
using KNote.Model.Dto;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using KNote.Model.Services;

namespace KNote.ServiceEF.Services
{
    public class KntUserService : DomainActionBase, IKntUserService
    {
        #region Fields

        private readonly IKntRepository _repository;

        #endregion

        #region Constructor

        protected internal KntUserService(IKntRepository repository)
        {
            _repository = repository;
        }

        #endregion

        #region IKntUserService  

        public async Task<Result<List<UserBaseDto>>> GetAllAsync(PaginationDto pagination = null)
        {
            var resService = new Result<List<UserBaseDto>>();
            try
            {   
                if(pagination != null)
                {
                    var query = _repository.Users.Queryable
                        .OrderBy(u => u.UserName)
                        .Pagination(pagination);
                    resService.Entity = await query
                        .Select(u => u.GetSimpleDto<UserBaseDto>())                    
                        .ToListAsync();                
                }
                else
                {
                    // Implementación old
                    //var resRep = await _repository.Users.GetAllAsync();
                    //resService.Entity = resRep.Entity?.Select(u => u.GetSimpleDto<UserBaseDto>()).ToList();
                    //resService.ErrorList = resRep.ErrorList;

                    // ó

                    var query = _repository.Users.Queryable
                        .OrderBy(u => u.UserName);
                    resService.Entity = await query
                        .Select(u => u.GetSimpleDto<UserBaseDto>())
                        .ToListAsync();
                }

            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<int>> GetCount()
        {
            var resService = new Result<int>();
            try
            {                
                resService.Entity = await _repository.Users.Queryable.CountAsync();                
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<UserDto>> GetAsync(Guid userId)
        {
            var resService = new Result<UserDto>();
            try
            {                
                var resRep = await _repository.Users.GetAsync((object) userId);
                
                resService.Entity = resRep.Entity?.GetSimpleDto<UserDto>();
                // KNote template ... load here aditionals properties for UserDto
                // ... 

                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<UserDto>> SaveAsync(UserDto entity)
        {
            Result<User> resRep = null;
            var resService = new Result<UserDto>();

            try
            {
                if (entity.UserId == Guid.Empty)
                {
                    entity.UserId = Guid.NewGuid();
                    var newEntity = new User();
                    newEntity.SetSimpleDto(entity);
                    resRep = await _repository.Users.AddAsync(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.Users.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.Users.ThrowKntException = false;
                    }
                    
                    resRep = await _repository.Users.GetAsync(entity.UserId);
                    var entityForUpdate = resRep.Entity;

                    if (flagThrowKntException == true)
                        _repository.Users.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = await _repository.Users.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new User();
                        newEntity.SetSimpleDto(entity);

                        resRep = await _repository.Users.AddAsync(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            // TODO: Valorar refactorizar los siguiente (este patrón está en varios sitios.
            resService.Entity = resRep.Entity?.GetSimpleDto<UserDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public async Task<Result<UserBaseDto>> DeleteAsync(Guid id)
        {
            var resService = new Result<UserBaseDto>();
            try
            {
                var resRep = await _repository.Users.GetAsync(id);
                if (resRep.IsValid)
                {
                    resRep = await _repository.Users.DeleteAsync(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<UserBaseDto>();
                    else
                        resService.ErrorList = resRep.ErrorList;
                }
                else
                    resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<UserDto> Authenticate(string username, string password)
        {
            var resService = new Result<UserDto>();

            // check if username and pwd is not null
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                resService.AddErrorMessage("User not authenticated");
                resService.Entity = null;
                return resService;
            }

            var resRep = _repository.Users.Get(u => u.UserName == username);

            // check if username exists
            if (!resRep.IsValid)
            {
                resService.AddErrorMessage("User not authenticated");
                resService.Entity = null;
                return resService;
            }

            // check if password is correct
            if (!VerifyPasswordHash(password, resRep.Entity.PasswordHash, resRep.Entity.PasswordSalt))
            {
                resService.AddErrorMessage("User not authenticated");
                resService.Entity = null;
                return resService;
            }

            // authentication successful
            resService.Entity = resRep.Entity?.GetSimpleDto<UserDto>();
            return resService;
        }
                
        public Result<UserDto> Create(UserRegisterDto userRegisterInfo)
        {
            Result<UserDto> resService = new Result<UserDto>();
            
            var password = userRegisterInfo.Password;

            try
            {
                if (string.IsNullOrWhiteSpace(password))
                    throw new AppException("Password is required");

                if (_repository.Users.Get(u => u.UserName == userRegisterInfo.UserName).Entity != null)
                    throw new AppException("Username \"" + userRegisterInfo.UserName + "\" is already taken");
                else
                {                                        
                    byte[] passwordHash, passwordSalt;
                    CreatePasswordHash(password, out passwordHash, out passwordSalt);

                    var newEntity = new User();                    
                    newEntity.SetSimpleDto(userRegisterInfo);
                    newEntity.UserId = Guid.NewGuid();
                    newEntity.PasswordHash = passwordHash;
                    newEntity.PasswordSalt = passwordSalt;

                    var resRep = _repository.Users.Add(newEntity);
                    resService.Entity = resRep.Entity?.GetSimpleDto<UserDto>();
                    if (!resRep.IsValid)
                        CopyErrorList(resRep.ErrorList, resService.ErrorList);
                }
            }
            catch (Exception ex)
            {
                if (resService == null)
                    resService = new Result<UserDto>();
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        #endregion

        #region Private helper methods

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                // TODO: remove (for debug and seed data).
                //var strSalt = Convert.ToBase64String(passwordSalt);
                //var strHash = Convert.ToBase64String(passwordHash);
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }

        #endregion 

    }
}
