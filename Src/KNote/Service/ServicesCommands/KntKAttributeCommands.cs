using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.Service.ServicesCommands;

public class KntKAttributesGetAllAsyncCommand : KntCommandServiceBase<Result<List<KAttributeInfoDto>>>
{
    public KntKAttributesGetAllAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<List<KAttributeInfoDto>>> Execute()
    {
        return await Repository.KAttributes.GetAllAsync();
    }
}

public class KntKAttributesGetAllByTypeAsyncCommand : KntCommandServiceBase<Guid?, Result<List<KAttributeInfoDto>>>
{
    public KntKAttributesGetAllByTypeAsyncCommand(IKntService service, Guid? typeId) : base(service, typeId)
    {

    }

    public override async Task<Result<List<KAttributeInfoDto>>> Execute()
    {
        return await Repository.KAttributes.GetAllAsync(Param);
    }
}

public class KntKAttributesGetAsyncCommand : KntCommandServiceBase<Guid, Result<KAttributeDto>>
{
    public KntKAttributesGetAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<KAttributeDto>> Execute()
    {
        return await Repository.KAttributes.GetAsync(Param);
    }
}

public class KntKAttributesSaveAsyncCommand : KntCommandSaveServiceBase<KAttributeDto, Result<KAttributeDto>>
{
    public KntKAttributesSaveAsyncCommand(IKntService service, KAttributeDto entity) : base(service, entity)
    {

    }

    public override async Task<Result<KAttributeDto>> Execute()
    {
        if (Param.KAttributeId == Guid.Empty)
        {
            Param.KAttributeId = Guid.NewGuid();
            return await Repository.KAttributes.AddAsync(Param);
        }
        else
        {
            return await Repository.KAttributes.UpdateAsync(Param);
        }
    }
}

public class KntKAttributesDeleteAsyncCommand : KntCommandServiceBase<Guid, Result<KAttributeInfoDto>>
{
    public KntKAttributesDeleteAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<KAttributeInfoDto>> Execute()
    {
        var result = new Result<KAttributeInfoDto>();

        var resGetEntity = await Service.KAttributes.GetAsync(Param);

        if (resGetEntity.IsValid)
        {
            var resDelEntity = await Repository.KAttributes.DeleteAsync(Param);
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
}

public class KntKAttributesTabulatedValuesAsyncCommand : KntCommandServiceBase<Guid, Result<List<KAttributeTabulatedValueDto>>>
{
    public KntKAttributesTabulatedValuesAsyncCommand(IKntService service, Guid attributeId) : base(service, attributeId)
    {

    }

    public override async Task<Result<List<KAttributeTabulatedValueDto>>> Execute()
    {
        return await Repository.KAttributes.GetKAttributeTabulatedValuesAsync(Param);
    }
}