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
    public class KntNoteTypeRepository : KntRepositoryBase, IKntNoteTypeRepository
    {
        public KntNoteTypeRepository(KntDbContext singletonContext, RepositoryRef repositoryRef)
            : base(singletonContext, repositoryRef)
        {
        }

        public KntNoteTypeRepository(RepositoryRef repositoryRef)
            : base(repositoryRef)
        {
        }

        public async Task<Result<List<NoteTypeDto>>> GetAllAsync()
        {            
            var response = new Result<List<NoteTypeDto>>();
            try
            {
                var ctx = GetOpenConnection();
                var noteTypes = new GenericRepositoryEF<KntDbContext, NoteType>(ctx);
                
                var resGenRep = await noteTypes.GetAllAsync();
                response.Entity = resGenRep.Entity?
                    .Select(t => t.GetSimpleDto<NoteTypeDto>())
                    .OrderBy(t => t.Name)
                    .ToList();
                response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        public async Task<Result<NoteTypeDto>> GetAsync(Guid id)
        {
            var response = new Result<NoteTypeDto>();
            try
            {
                var ctx = GetOpenConnection();
                var noteTypes = new GenericRepositoryEF<KntDbContext, NoteType>(ctx);

                var resGenRep = await noteTypes.GetAsync((object)id);

                response.Entity = resGenRep.Entity?.GetSimpleDto<NoteTypeDto>();
                response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        public async Task<Result<NoteTypeDto>> AddAsync(NoteTypeDto entity)
        {
            var response = new Result<NoteTypeDto>();
            try
            {
                var ctx = GetOpenConnection();
                var noteTypes = new GenericRepositoryEF<KntDbContext, NoteType>(ctx);

                var newEntity = new NoteType();
                newEntity.SetSimpleDto(entity);

                var resGenRep = await noteTypes.AddAsync(newEntity);

                response.Entity = resGenRep.Entity?.GetSimpleDto<NoteTypeDto>();
                response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        public async Task<Result<NoteTypeDto>> UpdateAsync(NoteTypeDto entity)
        {
            var resGenRep = new Result<NoteType>();            
            var response = new Result<NoteTypeDto>();

            try
            {
                var ctx = GetOpenConnection();
                var noteTypes = new GenericRepositoryEF<KntDbContext, NoteType>(ctx);

                var resGenRepGet = await noteTypes.GetAsync(entity.NoteTypeId) ;
                NoteType entityForUpdate;

                if (resGenRepGet.IsValid)
                {
                    entityForUpdate = resGenRepGet.Entity;
                    entityForUpdate.SetSimpleDto(entity);
                    resGenRep = await noteTypes.UpdateAsync(entityForUpdate);
                }
                else
                {
                    resGenRep.Entity = null;
                    resGenRep.AddErrorMessage("Can't find entity for update.");
                }
                
                response.Entity = resGenRep.Entity?.GetSimpleDto<NoteTypeDto>();
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
                var noteTypes = new GenericRepositoryEF<KntDbContext, NoteType>(ctx);

                var resGenRep = await noteTypes.DeleteAsync(id);
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
