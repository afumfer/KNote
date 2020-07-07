using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Repository.EntityFramework
{
    public class KntUserRepository : DomainActionBase, IKntUserRepository
    {
        private IGenericRepositoryEF<KntDbContext, User> _users;

        public KntUserRepository(KntDbContext context, bool throwKntException)
        {
            _users = new GenericRepositoryEF<KntDbContext, User>(context, throwKntException);
        }

        public Task<Result<UserDto>> DeleteAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<UserDto>>> GetAllAsync(PaginationDto pagination = null)
        {
            throw new NotImplementedException();
        }

        public Task<Result<UserDto>> GetAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<Result<int>> GetCount()
        {
            throw new NotImplementedException();
        }

        public Task<Result<UserDto>> SaveAsync(UserDto entityInfo)
        {
            throw new NotImplementedException();
        }

        #region  IDisposable

        public virtual void Dispose()
        {
            _users.Dispose();
        }

        #endregion
    }
}
