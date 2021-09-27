using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using System.Linq.Expressions;
using KNote.Service;
using KNote.Repository;

namespace KNote.Service.Services
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

        public async Task<Result<List<UserDto>>> GetAllAsync(PageIdentifier pagination = null)
        {
            return await _repository.Users.GetAllAsync(pagination);
        }

        public async Task<Result<long>> GetCount()
        {
            return await _repository.Users.GetCount();
        }

        public async Task<Result<UserDto>> GetAsync(Guid userId)
        {
            return await _repository.Users.GetAsync(userId);
        }

        public async Task<Result<UserDto>> GetByUserNameAsync(string userName)
        {
            return await _repository.Users.GetByUserNameAsync(userName);
        }

        public async Task<Result<UserDto>> SaveAsync(UserDto entity)
        {            
            if (entity.UserId == Guid.Empty)
            {
                entity.UserId = Guid.NewGuid();
                return await _repository.Users.AddAsync(entity);
            }
            else
            {
                return await _repository.Users.UpdateAsync(entity);
            }
        }

        public async Task<Result<UserDto>> DeleteAsync(Guid id)
        {            
            var result = new Result<UserDto>();

            var resGetEntity = await GetAsync(id);

            if (resGetEntity.IsValid)
            {
                var resDelEntity = await _repository.Users.DeleteAsync(id);
                if (resDelEntity.IsValid)
                    result.Entity = resGetEntity.Entity;
                else
                    result.ErrorList = resDelEntity.ErrorList;
            }
            else
            {
                result.ErrorList = resGetEntity.ErrorList;
            }

            return result;
        }

        public async Task<Result<UserDto>> Authenticate(string username, string password)
        {
            var resService = new Result<UserDto>();

            // check if username and pwd is not null
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                resService.AddErrorMessage("User not authenticated");
                resService.Entity = null;
                return resService;
            }

            //var resRep = _repository.Users.Get(u => u.UserName == username);
            var resRep = await _repository.Users.GetInternalAsync(username);

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
                
        public async Task<Result<UserDto>> Create(UserRegisterDto userRegisterInfo)
        {
            Result<UserDto> resService = new Result<UserDto>();
            
            var password = userRegisterInfo.Password;

            try
            {
                if (string.IsNullOrWhiteSpace(password))
                    throw new AppException("Password is required");
                
                if ((await _repository.Users.GetInternalAsync(userRegisterInfo.UserName)).Entity != null)
                        throw new AppException("Username \"" + userRegisterInfo.UserName + "\" is already taken");
                else
                {                                        
                    byte[] passwordHash, passwordSalt;
                    CreatePasswordHash(password, out passwordHash, out passwordSalt);

                    var newEntity = new UserInternalDto();                    
                    newEntity.SetSimpleDto(userRegisterInfo);
                    newEntity.UserId = Guid.NewGuid();
                    newEntity.PasswordHash = passwordHash;
                    newEntity.PasswordSalt = passwordSalt;
                    
                    var resRep = await _repository.Users.AddInternalAsync(newEntity);
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
