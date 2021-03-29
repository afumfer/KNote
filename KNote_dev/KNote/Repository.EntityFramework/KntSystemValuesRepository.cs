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
    public class KntSystemValuesRepository : KntRepositoryBase, IKntSystemValuesRepository
    {        

        public KntSystemValuesRepository(KntDbContext singletonContext, RepositoryRef repositoryRef, bool throwKntException)
            : base(singletonContext, repositoryRef, throwKntException)
        {
        }

        public KntSystemValuesRepository(RepositoryRef repositoryRef, bool throwKntException = false)
            : base(repositoryRef, throwKntException)
        {
        }

        public async Task<Result<List<SystemValueDto>>> GetAllAsync()
        {
            var resService = new Result<List<SystemValueDto>>();
            try
            {
                var ctx = GetOpenConnection();
                var systemValues = new GenericRepositoryEF<KntDbContext, SystemValue>(ctx, ThrowKntException);

                var resRep = await systemValues.GetAllAsync();
                resService.Entity = resRep.Entity?.Select(sv => sv.GetSimpleDto<SystemValueDto>()).ToList();
                resService.ErrorList = resRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<SystemValueDto>> GetAsync(string scope, string key)
        {
            var resService = new Result<SystemValueDto>();
            try
            {
                var ctx = GetOpenConnection();
                var systemValues = new GenericRepositoryEF<KntDbContext, SystemValue>(ctx, ThrowKntException);

                var resRep = await systemValues.GetAsync(sv => sv.Scope == scope && sv.Key == key);
                resService.Entity = resRep.Entity?.GetSimpleDto<SystemValueDto>();
                resService.ErrorList = resRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<SystemValueDto>> GetAsync(Guid id)
        {
            var resService = new Result<SystemValueDto>();
            try
            {
                var ctx = GetOpenConnection();
                var systemValues = new GenericRepositoryEF<KntDbContext, SystemValue>(ctx, ThrowKntException);

                var resRep = await systemValues.GetAsync((object)id);
                resService.Entity = resRep.Entity?.GetSimpleDto<SystemValueDto>();
                resService.ErrorList = resRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<SystemValueDto>> AddAsync(SystemValueDto entity)
        {
            var response = new Result<SystemValueDto>();
            try
            {
                var ctx = GetOpenConnection();
                var systemValues = new GenericRepositoryEF<KntDbContext, SystemValue>(ctx, ThrowKntException);

                var newEntity = new SystemValue();
                newEntity.SetSimpleDto(entity);

                var resGenRep = await systemValues.AddAsync(newEntity);

                response.Entity = resGenRep.Entity?.GetSimpleDto<SystemValueDto>();
                response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        public async Task<Result<SystemValueDto>> UpdateAsync(SystemValueDto entity)
        {
            var resGenRep = new Result<SystemValue>();
            var response = new Result<SystemValueDto>();

            try
            {
                var ctx = GetOpenConnection();
                var systemValues = new GenericRepositoryEF<KntDbContext, SystemValue>(ctx, ThrowKntException);

                bool flagThrowKntException = false;
                if (systemValues.ThrowKntException == true)
                {
                    flagThrowKntException = true;
                    systemValues.ThrowKntException = false;
                }

                var resGenRepGet = await systemValues.GetAsync(entity.SystemValueId);
                SystemValue entityForUpdate;

                if (flagThrowKntException == true)
                    systemValues.ThrowKntException = true;

                if (resGenRepGet.IsValid)
                {
                    entityForUpdate = resGenRepGet.Entity;
                    entityForUpdate.SetSimpleDto(entity);
                    resGenRep = await systemValues.UpdateAsync(entityForUpdate);
                }
                else
                {
                    resGenRep.Entity = null;
                    resGenRep.AddErrorMessage("Can't find entity for update.");
                }

                response.Entity = resGenRep.Entity?.GetSimpleDto<SystemValueDto>();
                response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }

            return ResultDomainAction(response);
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var response = new Result();
            try
            {
                var ctx = GetOpenConnection();
                var systemValues = new GenericRepositoryEF<KntDbContext, SystemValue>(ctx, ThrowKntException);

                var resGenRep = await systemValues.DeleteAsync(id);
                if (!resGenRep.IsValid)
                    response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);

        }
    }
}
