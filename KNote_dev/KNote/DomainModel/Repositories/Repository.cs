using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//using System.Data.Entity.Validation;
//using System.Data.Entity;
using Microsoft.EntityFrameworkCore;

using KNote.Shared;
using KNote.DomainModel.Infrastructure;
using KNote.Shared.Entities;
using System.Data.Common;
using System.ComponentModel;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KNote.DomainModel.Repositories
{
 
    public class Repository<TContext, TEntity> : DomainActionBase, IRepository<TContext, TEntity>
        where TEntity : ModelBase, new()
        where TContext: DbContext, new()
    {

        #region Private members
                
        private readonly TContext _context;
        
        internal readonly DbSet<TEntity> _dbSet;
        
        #endregion

        #region Constructor

        public Repository(string strConn, string strProvider = "System.Data.SqlClient", bool throwKntException = false)
            : base (throwKntException)
        {
            var conn = DbProviderFactories.GetFactory(strProvider).CreateConnection();
            conn.ConnectionString = strConn;            
            _context = Activator.CreateInstance(typeof(TContext), conn) as TContext;
            _dbSet = _context.Set<TEntity>();
        }

        public Repository(DbConnection openConn, bool throwKntException = false)
            : base(throwKntException)
        {            
            _context = Activator.CreateInstance(typeof(TContext), openConn) as TContext;
            _dbSet = _context.Set<TEntity>();
        }

        public Repository(TContext context, bool throwKntException = false)
            : base(throwKntException)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        #endregion

        #region Generics methods
        

        public Result<TEntity> Get(params object[] keyValues)
        {
            var result = new Result<TEntity>();
            TEntity dbEntry = null;

            try
            {
                dbEntry = _context.Set<TEntity>().Find(keyValues);
                if (dbEntry == null)
                    result.AddErrorMessage("Entity not found.");

                result.Entity = dbEntry;

            }
            catch (KntEntityValidationException ex)
            {
                AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction<TEntity>(result);
        }

        public Result<TEntity> Get(Expression<Func<TEntity, bool>> predicate)
        {
            var result = new Result<TEntity>();
            TEntity dbEntry = null;

            try
            {
                // FirstOrDefault or SingleOrDefault ?
                
                dbEntry = _context.Set<TEntity>().Where(predicate)
                    .FirstOrDefault();

                if (dbEntry == null)
                    result.AddErrorMessage("Entity not found.");

                result.Entity = dbEntry;
            }
            catch (KntEntityValidationException ex)
            {
                AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction<TEntity>(result);
        }


        // TODO: Método pendiente de probar. 
        //public virtual Result<IEnumerable<TEntity>> GetWithRawSql(string query, params object[] parameters)
        public Result<IEnumerable<TEntity>> GetWithRawSql(string query, params object[] parameters)
        {
            var result = new Result<IEnumerable<TEntity>>();
            // TODO: SqlQuery no existe en entityframeworkcore
            //result.Entity = _dbSet.SqlQuery(query, parameters).ToList();
            return result;            
        }

        // TODO: Método pendiente de probar (Igual que el anterior, ¿debería ser virtual?)
        public Result<IEnumerable<TEntity>> GetV2(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            var result = new Result<IEnumerable<TEntity>>();

            IQueryable<TEntity> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            if (orderBy != null)
            {                
                result.Entity = orderBy(query).ToList();
            }
            else
            {                
                result.Entity = query.ToList();
            }

            return result;
        }


        public bool IsAttached(Func<TEntity, bool> predicate)
        {
            return _context.Set<TEntity>().Local.Any(predicate);
        }

        public void RemoveLocal(TEntity entity)
        {
            _context.Set<TEntity>().Local.Remove(entity);
        }


        public async Task<Result<TEntity>> GetAsync(params object[] keyValues)
        {
            var result = new Result<TEntity>();
            TEntity dbEntry = null;

            try
            {
                dbEntry = await _context.Set<TEntity>().FindAsync(keyValues);
                if (dbEntry == null)
                    result.AddErrorMessage("Entity not found.");

                result.Entity = dbEntry;
            }
            catch (KntEntityValidationException ex)
            {
                AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction<TEntity>(result);
        }

        public Result<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> predicate = null)
        {
            var result = new Result<List<TEntity>>();

            try
            {
                if (predicate == null)
                    // TODO... quitar el take(500), sólo para pruebas
                    result.Entity = _context.Set<TEntity>()
                        .ToList();
                else
                    result.Entity = _context.Set<TEntity>().Where(predicate)
                        .ToList();
            }
            catch (KntEntityValidationException ex)
            {
                AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction<List<TEntity>>(result);
        }

        public Result<List<TEntity>> GetAllWithPagination(int page, int limit, Expression<Func<TEntity, bool>> predicate = null)
        {
            var result = new Result<List<TEntity>>();

            try
            {
                if (predicate == null)                    
                    result.Entity = _context.Set<TEntity>().OrderBy(n => n.Timestamp)
                        .Skip((page-1)*limit).Take(limit)
                        .ToList();
                else
                    result.Entity = _context.Set<TEntity>().Where(predicate).OrderBy(e => e.Timestamp)
                        .Skip((page-1)*limit).Take(limit)
                        .ToList();
            }
            catch (KntEntityValidationException ex)
            {
                AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction<List<TEntity>>(result);
        }

        public Result<TEntity> Add(TEntity entity)
        {
            var result = new Result<TEntity>();
            
            int res;

            try
            {                
                _context.Set<TEntity>().Add(entity);
                res = _context.SaveChanges();
                result.Entity = _context.Entry(entity).Entity;
            }
            catch (KntEntityValidationException ex)
            {
                AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction<TEntity>(result);
        }

        // TODO: Pendiente de implementar
        public Result<IEnumerable<TEntity>> AddRange(IEnumerable<TEntity> entity)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<TEntity>> AddAsync(TEntity entity)
        {
            var result = new Result<TEntity>();
            int res;

            try
            {
                await _context.Set<TEntity>().AddAsync(entity);
                res = await _context.SaveChangesAsync();
                result.Entity = _context.Entry<TEntity>(entity).Entity;
            }
            catch (KntEntityValidationException ex)
            {
                AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction<TEntity>(result);
        }

        public Result<TEntity> Update(TEntity entity)
        {
            var result = new Result<TEntity>();
            int res;

            try
            {                
                res = _context.SaveChanges();
                result.Entity = _context.Entry(entity).Entity;
            }
            catch (KntEntityValidationException ex)
            {
                AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction<TEntity>(result);
        }

        public async Task<Result<TEntity>> UpdateAsync(TEntity entity)
        {
            var result = new Result<TEntity>();
            int res;

            try
            {
                res = await  _context.SaveChangesAsync();
                result.Entity = _context.Entry<TEntity>(entity).Entity;
            }
            catch (KntEntityValidationException ex)
            {
                AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction<TEntity>(result);
        }

        public Result<TEntity> Delete(params object[] keyValues)
        {
            var result = new Result<TEntity>();
            int res;
            TEntity dbEntry = null;

            try
            {
                dbEntry = _context.Set<TEntity>().Find(keyValues);

                if (dbEntry != null)
                {
                    _context.Set<TEntity>().Remove(dbEntry);
                }
                res = _context.SaveChanges();
                result.Entity = dbEntry;
            }
            catch (KntEntityValidationException ex)
            {
                AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction<TEntity>(result);
        }

        public async Task<Result<TEntity>> DeleteAsync(params object[] keyValues)
        {
            var result = new Result<TEntity>();
            int res;
            TEntity dbEntry = null;

            try
            {
                dbEntry = await _context.Set<TEntity>().FindAsync(keyValues);

                if (dbEntry != null)
                {
                    _context.Set<TEntity>().Remove(dbEntry);
                }
                res = await _context.SaveChangesAsync();
                result.Entity = dbEntry;
            }
            catch (KntEntityValidationException ex)
            {
                AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction<TEntity>(result);
        }

        public Result<TEntity> Delete(TEntity entity)
        {
            var result = new Result<TEntity>();
            int res;

            try
            {
                result.Entity = entity;
                _context.Set<TEntity>().Remove(entity);               
                res = _context.SaveChanges();
            }
            catch (KntEntityValidationException ex)
            {
                AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction<TEntity>(result);
        }

        public async Task<Result<TEntity>> DeleteAsync(TEntity entity)
        {
            var result = new Result<TEntity>();
            int res;

            try
            {
                result.Entity = entity;
                _context.Set<TEntity>().Remove(entity);
                res = await _context.SaveChangesAsync();
            }
            catch (KntEntityValidationException ex)
            {
                AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction<TEntity>(result);
        }

        public Result<IEnumerable<TEntity>> DeleteRange(IEnumerable<TEntity> entities)
        {
            var result = new Result<IEnumerable<TEntity>>();
            int res;

            try
            {
                result.Entity = entities;
                _context.Set<TEntity>().RemoveRange(entities);                
                res = _context.SaveChanges();
            }
            catch (KntEntityValidationException ex)
            {
                AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction<IEnumerable<TEntity>>(result);
        }

        public async Task<Result<IEnumerable<TEntity>>> DeleteRangeAsync(IEnumerable<TEntity> entities)
        {
            var result = new Result<IEnumerable<TEntity>>();
            int res;

            try
            {
                result.Entity = entities;
                _context.Set<TEntity>().RemoveRange(entities);
                res = await _context.SaveChangesAsync();
            }
            catch (KntEntityValidationException ex)
            {
                AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction<IEnumerable<TEntity>>(result);
        }

        public Result<TEntity> LoadCollection<TCollection>(TEntity entity, Expression<Func<TEntity, IEnumerable<TCollection>>> colec)
            where TCollection : ModelBase
        {
            var result = new Result<TEntity>();
            
            try
            {
                _context.Entry(entity).Collection<TCollection>(colec).Load();        
                result.Entity = entity;
            }
            catch (KntEntityValidationException ex)
            {
                AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction<TEntity>(result);
        }

        public Result<TEntity> LoadReference<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> prop)
            where TProperty : class
        {
            var result = new Result<TEntity>();

            try
            {
                //_context.Entry(entity).Collection<TCollection>(colec).Load();
                _context.Entry(entity).Reference(prop).Load();
                result.Entity = entity;
            }
            catch (KntEntityValidationException ex)
            {
                AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction<TEntity>(result);
        }


        #endregion

        #region Public properties

        public DbSet<TEntity> DbSet
        {
            get { return _dbSet; }
        }

        public IQueryable<TEntity> Queryable
        {
            get { return _context.Set<TEntity>().AsQueryable(); }

        }

        #endregion

        #region  IDisposable

        public virtual void Dispose()
        {
            _context.Dispose();
        }

        #endregion
      
    }
}
