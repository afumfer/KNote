using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using System.Linq.Expressions;
using KNote.Repository;
using KNote.Service.Interfaces;
using KNote.Service.Core;

namespace KNote.Service.Services
{
    public class KntKAttributeService : KntServiceBase, IKntKAttributeService
    {
        #region Constructor

        public KntKAttributeService(IKntService service) : base(service)
        {

        }

        #endregion

        #region IKntAttributeService

        public async Task<Result<List<KAttributeInfoDto>>> GetAllAsync()
        {
            return await Repository.KAttributes.GetAllAsync();
        }

        public async Task<Result<List<KAttributeInfoDto>>> GetAllAsync(Guid? typeId)
        {
            return await Repository.KAttributes.GetAllAsync(typeId);
        }

        public async Task<Result<KAttributeDto>> GetAsync(Guid id)
        {
            return await Repository.KAttributes.GetAsync(id);
        }

        public async Task<Result<KAttributeDto>> SaveAsync(KAttributeDto entity)
        {            
            if (entity.KAttributeId == Guid.Empty)
            {
                entity.KAttributeId = Guid.NewGuid();
                return await Repository.KAttributes.AddAsync(entity);
            }
            else
            {                
                return await Repository.KAttributes.UpdateAsync(entity);
            }
        }

        public async Task<Result<KAttributeInfoDto>> DeleteAsync(Guid id)
        {           
            var result = new Result<KAttributeInfoDto>();

            var resGetEntity = await GetAsync(id);

            if (resGetEntity.IsValid)
            {
                var resDelEntity = await Repository.KAttributes.DeleteAsync(id);
                if (resDelEntity.IsValid)
                    result.Entity = resGetEntity.Entity;
                else
                    result.AddListErrorMessage(resDelEntity.ListErrorMessage);
            }
            else
            {
                result.AddListErrorMessage(resGetEntity.ListErrorMessage);
            }

            return result;
        }

        public async Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid attributeId)
        {
            return await Repository.KAttributes.GetKAttributeTabulatedValuesAsync(attributeId);
        }

        #endregion 
    }
}
