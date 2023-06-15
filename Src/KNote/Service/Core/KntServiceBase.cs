using System;
using System.Reflection;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Repository;
using Microsoft.Extensions.Logging;

namespace KNote.Service.Core;

public abstract class KntServiceBase
{
    private readonly IKntService _service;
    internal IKntService Service
    {
        get { return _service; }
    }

    internal IKntRepository Repository
    {
        get { return _service.Repository; }
    }

    public KntServiceBase(IKntService service)
    {
        _service = service;
    }

    public async Task<TResult> ExecuteCommand<TParam, TResult>(KntCommandServiceBase<TParam, TResult> command) where TResult : ResultBase, new() 
    {        
        try
        {
            TResult result;
            var validParam = command.ValidateParam();
            if (validParam.IsValid)
            {
                Service.Logger?.LogTrace("Service Validate param - {param} is valid", command.Param);
                result = await ExecuteCommand<TResult>(command);
            }
            else
            {
                result = new TResult();
                result.AddErrorMessage("Invalid param. ");
                result.AddListErrorMessage(validParam.ListErrorMessage);
                Service.Logger?.LogTrace("Service Validate - {param} is not valid, errors: {errorMessage}", command.Param, validParam.ErrorMessage);
                return result;
            }            
            return result;
        }
        catch (Exception ex)
        {
            Service.Logger?.LogError(ex, "Service ExecuteCommand {command}", MethodBase.GetCurrentMethod().DeclaringType);
            throw new KntServiceException($"KNote service error.  ({MethodBase.GetCurrentMethod().DeclaringType}). ", ex);
        }
    }

    public async Task<TResult> ExecuteCommand<TResult>(KntCommandServiceBase<TResult> command) where TResult : ResultBase, new()
    {
        try            
        {                        
            TResult result;
            var validAuthorization = command.ValidateAuthorization();
            if (validAuthorization.IsValid)
            {
                Service.Logger?.LogTrace("Service validated authorization for {command}", command.GetType());
                result = await command.Execute();
                Service.Logger?.LogTrace("Service ExecutedCommand {command}", command.GetType());
            }
            else
            {
                result = new TResult();
                result.AddErrorMessage("Not authorized. ");
                result.AddListErrorMessage(validAuthorization.ListErrorMessage);
                Service.Logger?.LogTrace("Service authorization is not valid for {command}", command.GetType());
            }                   
            return result;
        }
        catch (Exception ex)
        {
            Service.Logger?.LogError(ex, "Service ExecuteCommand {command}", MethodBase.GetCurrentMethod().DeclaringType);
            throw new KntServiceException($"KNote service error.  ({MethodBase.GetCurrentMethod().DeclaringType}). ", ex);
        }        
    }
}
