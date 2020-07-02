using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Linq;

using KNote.Model;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KNote.ServiceEF.Repositories
{
    public interface IRepository<TContext, TEntity> : IDisposable
        where TEntity : ModelBase, new()
        where TContext : DbContext, new()
    {
        #region Generics properties

        bool ThrowKntException { get; set; }                

        DbSet<TEntity> DbSet { get; }

        IQueryable<TEntity> Queryable { get; }

        #endregion

        #region Generics entity methods        

        bool IsAttached(Func<TEntity, bool> predicate);
        void RemoveLocal(TEntity entity);

        Result<TEntity> Get(params object[] keyValues);
        Result<TEntity> Get(Expression<Func<TEntity, bool>> predicate);        
        Task<Result<TEntity>> GetAsync(params object[] keyValues);
        Task<Result<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate);
        Result<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> predicate = null);
        Task<Result<List<TEntity>>> GetAllAsync(Expression<Func<TEntity, bool>> predicate = null);
        Result<List<TEntity>> GetAllWithPagination(int page, int limit, Expression<Func<TEntity, bool>> predicate = null);
        
        Task<Result<IEnumerable<TEntity>>> GetAllAsyncExt(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,  
            string includeProperties = "");                        
        
        Result<TEntity> Add(TEntity entity);
        Result<IEnumerable<TEntity>> AddRange(IEnumerable<TEntity> entity);
        Task<Result<TEntity>> AddAsync(TEntity entity);
        Result<TEntity> Update(TEntity entity);
        Task<Result<TEntity>> UpdateAsync(TEntity entity);
        Result<TEntity> Delete(params object[] keyValues);
        Result<TEntity> Delete(TEntity entity);
        Task<Result<TEntity>> DeleteAsync(TEntity entity);
        Result<IEnumerable<TEntity>> DeleteRange(IEnumerable<TEntity> entity);
        Task<Result<IEnumerable<TEntity>>> DeleteRangeAsync(IEnumerable<TEntity> entity);        
        Result<TEntity> LoadCollection<TCollection>(TEntity entity, Expression<Func<TEntity, IEnumerable<TCollection>>> colec) where TCollection : ModelBase;
        Result<TEntity> LoadReference<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> prop) where TProperty : class;
                
        #endregion
    }
}
