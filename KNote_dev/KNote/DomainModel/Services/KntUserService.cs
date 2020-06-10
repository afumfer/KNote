using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.DomainModel.Repositories;
using KNote.Shared;
using KNote.DomainModel.Entities;
using KNote.Shared.Dto;
using KNote.Shared.Dto.Info;
using KNote.DomainModel.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace KNote.DomainModel.Services
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

        public Result<List<UserInfoDto>> GetAll()
        {
            var resService = new Result<List<UserInfoDto>>();           
            try
            {                
                var resRep = _repository.Users.GetAll();
                resService.Entity = resRep.Entity?.Select(u => u.GetSimpleDto<UserInfoDto>()).ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {                
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<List<UserInfoDto>>> GetAllAsync(PaginationDto pagination)
        {
            var resService = new Result<List<UserInfoDto>>();
            try
            {                
                var query = _repository.Users.Queryable
                    .OrderBy(u => u.UserName)
                    .Pagination(pagination);
                
                resService.Entity = await query
                    .Select(u => u.GetSimpleDto<UserInfoDto>())                    
                    .ToListAsync();                
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

        public Result<UserDto> Get(string userName)
        {
            var resService = new Result<UserDto>();
            try
            {
                var resRep = _repository.Users.Get(u => u.UserName == userName);
                
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

        public Result<UserDto> GetMessages(Guid id)
        {
            var resService = new Result<UserDto>();
            try
            {
                var resRep = _repository.Users.Get((object) id);
                if (!resRep.IsValid)
                    CopyErrorList(resRep.ErrorList, resService.ErrorList);
                resRep = _repository.Users.LoadCollection(resRep.Entity, u => u.KMessages);
                if (!resRep.IsValid)
                    CopyErrorList(resRep.ErrorList, resService.ErrorList);
                //
                resService.Entity = resRep.Entity?.GetSimpleDto<UserDto>();                                
                resService.Entity.MessagesInfo = resRep.Entity.KMessages.Select(m => m.GetSimpleDto<KMessageDto>()).ToList();                                
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<UserDto> New(UserInfoDto entityInfo = null)
        {
            var resService = new Result<UserDto>();
            UserDto newUser;

            try
            {
                newUser = new UserDto();
                if (entityInfo != null)
                    newUser.SetSimpleDto(entityInfo);

                if (newUser.UserId == Guid.Empty)
                    newUser.UserId = Guid.NewGuid();

                // TODO: load default values
                // for newUser

                resService.Entity = newUser;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            return ResultDomainAction(resService);
        }

        public Result<UserDto> Save(UserDto entity)
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
                  
                    // TODO: update standard control values to newEntity
                    // ...

                    resRep = _repository.Users.Add(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.Users.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.Users.ThrowKntException = false;
                    }

                    var entityForUpdate = _repository.Users.Get(entity.UserId).Entity;

                    if (flagThrowKntException == true)
                        _repository.Users.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = _repository.Users.Update(entityForUpdate);
                    }
                    else
                    {                        
                        var newEntity = new User();
                        newEntity.SetSimpleDto(entity);

                        // TODO: update standard control values to newEntity
                        // ...
                        resRep = _repository.Users.Add(newEntity);
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

                    // TODO: update standard control values to newEntity
                    // ...

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

                    //var entityForUpdate = _repository.Users.Get(entityInfo.UserId).Entity;
                    resRep = await _repository.Users.GetAsync(entity.UserId);
                    var entityForUpdate = resRep.Entity;

                    if (flagThrowKntException == true)
                        _repository.Users.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = await _repository.Users.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new User();
                        newEntity.SetSimpleDto(entity);

                        // TODO: update standard control values to newEntity
                        // ...
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

        public async Task<Result<UserInfoDto>> DeleteAsync(Guid id)
        {
            var resService = new Result<UserInfoDto>();
            try
            {
                var resRep = await _repository.Users.GetAsync(id);
                if (resRep.IsValid)
                {
                    resRep = await _repository.Users.DeleteAsync(resRep.Entity);
                    if (resRep.IsValid)                    
                        resService.Entity = resRep.Entity?.GetSimpleDto<UserInfoDto>();                                            
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

        public Result<UserInfoDto> Delete(Guid id)
        {
            var resService = new Result<UserInfoDto>();
            try
            {
                var resRep = _repository.Users.Get(id);
                if (resRep.IsValid)
                {
                    resRep = _repository.Users.Delete(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<UserInfoDto>();
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
