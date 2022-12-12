using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.EntityFramework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Repository.EntityFramework
{    
    public class KntSystemValuesRepository : KntRepositoryBase, IKntSystemValuesRepository
    {        

        public KntSystemValuesRepository(KntDbContext singletonContext, RepositoryRef repositoryRef)
            : base(singletonContext, repositoryRef)
        {
        }

        public KntSystemValuesRepository(RepositoryRef repositoryRef)
            : base(repositoryRef)
        {
        }

        public async Task<Result<List<SystemValueDto>>> GetAllAsync()
        {
            var resService = new Result<List<SystemValueDto>>();
            try
            {
                var ctx = GetOpenConnection();
                var systemValues = new GenericRepositoryEF<KntDbContext, SystemValue>(ctx);

                var resRep = await systemValues.GetAllAsync();
                resService.Entity = resRep.Entity?.Select(sv => sv.GetSimpleDto<SystemValueDto>()).ToList();
                resService.AddListErrorMessage(resRep.ListErrorMessage);

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToResult(ex, resService);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<SystemValueDto>> GetAsync(string scope, string key)
        {
            var resService = new Result<SystemValueDto>();
            try
            {
                var ctx = GetOpenConnection();
                var systemValues = new GenericRepositoryEF<KntDbContext, SystemValue>(ctx);

                var resRep = await systemValues.GetAsync(sv => sv.Scope == scope && sv.Key == key);
                resService.Entity = resRep.Entity?.GetSimpleDto<SystemValueDto>();
                resService.AddListErrorMessage(resRep.ListErrorMessage);
                

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToResult(ex, resService);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<SystemValueDto>> GetAsync(Guid id)
        {
            var resService = new Result<SystemValueDto>();
            try
            {
                var ctx = GetOpenConnection();
                var systemValues = new GenericRepositoryEF<KntDbContext, SystemValue>(ctx);

                var resRep = await systemValues.GetAsync((object)id);
                resService.Entity = resRep.Entity?.GetSimpleDto<SystemValueDto>();
                resService.AddListErrorMessage(resRep.ListErrorMessage);

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToResult(ex, resService);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<SystemValueDto>> AddAsync(SystemValueDto entity)
        {
            var response = new Result<SystemValueDto>();
            try
            {
                var ctx = GetOpenConnection();
                var systemValues = new GenericRepositoryEF<KntDbContext, SystemValue>(ctx);

                var newEntity = new SystemValue();
                newEntity.SetSimpleDto(entity);

                var resGenRep = await systemValues.AddAsync(newEntity);

                response.Entity = resGenRep.Entity?.GetSimpleDto<SystemValueDto>();
                response.AddListErrorMessage(resGenRep.ListErrorMessage);

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToResult(ex, response);
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
                var systemValues = new GenericRepositoryEF<KntDbContext, SystemValue>(ctx);

                var resGenRepGet = await systemValues.GetAsync(entity.SystemValueId);
                SystemValue entityForUpdate;

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
                response.AddListErrorMessage(resGenRep.ListErrorMessage);

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToResult(ex, response);
            }

            return ResultDomainAction(response);
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var response = new Result();
            try
            {
                var ctx = GetOpenConnection();
                var systemValues = new GenericRepositoryEF<KntDbContext, SystemValue>(ctx);

                var resGenRep = await systemValues.DeleteAsync(id);
                if (!resGenRep.IsValid)
                    response.AddListErrorMessage(resGenRep.ListErrorMessage);

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToResult(ex, response);
            }
            return ResultDomainAction(response);

        }
    }
}
