using System;
using System.Collections.Generic;
using System.Linq;
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
        // The DB unique index covers (Name, NoteTypeId) - see ModelBuilderExtensions and
        // KntSchemaUpdater revision 3 - which no longer blocks duplicate Names among *global*
        // attributes (NoteTypeId == null): SQL Server/Sqlite treat each NULL as distinct in a unique
        // index. Checked here instead, only for that one case - attributes scoped to a NoteTypeId
        // stay covered by the DB index.
        if (Param.NoteTypeId == null)
        {
            var resGlobal = await Repository.KAttributes.GetAllIncludeNullTypeAsync(null);
            if (resGlobal.IsValid && resGlobal.Entity.Any(a =>
                    a.KAttributeId != Param.KAttributeId &&
                    string.Equals(a.Name, Param.Name, StringComparison.OrdinalIgnoreCase)))
            {
                var result = new Result<KAttributeDto>();
                result.AddErrorMessage($"An attribute named \"{Param.Name}\" already exists.");
                return result;
            }
        }

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

        if (!resGetEntity.IsValid)
        {
            result.AddListErrorMessage(resGetEntity.ListErrorMessage);
            return result;
        }

        // Business rule, checked here instead of letting it surface as a raw FK-constraint DB
        // error: an attribute still holding values on existing notes can't be deleted. Living in
        // the command (not in a caller like ClientWin's AttributeEditorCtrl) means every consumer
        // of this service - ClientWin and Server/Blazor's AttributesController alike - gets the
        // same clear message. Mirrors KntNoteTypeDeleteAsyncCommand's equivalent check.
        var resCountUsages = await Repository.KAttributes.CountNoteUsagesAsync(Param);
        if (resCountUsages.IsValid && resCountUsages.Entity > 0)
        {
            result.AddErrorMessage($"Can't delete this attribute: {resCountUsages.Entity} note(s) still use it.");
            return result;
        }

        var resDelEntity = await Repository.KAttributes.DeleteAsync(Param);
        if (resDelEntity.IsValid)
            result.Entity = resGetEntity.Entity;
        else
            result.AddListErrorMessage(resDelEntity.ListErrorMessage);

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