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
using KNote.Service.ServicesCommands;
using static Dapper.SqlMapper;

namespace KNote.Service.Services;

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
        var command = new KntKAttributesGetAllAsyncCommand(Service);
        return await ExecuteCommand(command);
    }

    public async Task<Result<List<KAttributeInfoDto>>> GetAllAsync(Guid? typeId)
    {     
        var command = new KntKAttributesGetAllByTypeAsyncCommand(Service, typeId);
        return await ExecuteCommand(command);
    }

    public async Task<Result<KAttributeDto>> GetAsync(Guid id)
    {
        var command = new KntKAttributesGetAsyncCommand(Service, id);
        return await ExecuteCommand(command);
    }

    public async Task<Result<KAttributeDto>> SaveAsync(KAttributeDto entity)
    {
        var command = new KntKAttributesSaveAsyncCommand(Service, entity);
        return await ExecuteCommand(command);
    }

    public async Task<Result<KAttributeInfoDto>> DeleteAsync(Guid id)
    {
        var command = new KntKAttributesDeleteAsyncCommand(Service, id);
        return await ExecuteCommand(command);
    }

    public async Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid attributeId)
    {
        var command = new KntKAttributesTabulatedValuesAsyncCommand(Service, attributeId);
        return await ExecuteCommand(command);
    }

    #endregion 
}
