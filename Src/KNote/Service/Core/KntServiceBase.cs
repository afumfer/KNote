using System;
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

    // No try/catch here: ValidateParam() cannot fail once the param is valid, and any exception raised while
    // actually executing the command is already caught, logged, and wrapped by ExecuteCommand<TResult> below.
    // Wrapping it again here used to double-wrap the exception, hiding the real cause behind two layers of
    // generic "KNote service error" messages.
    public async Task<TResult> ExecuteCommand<TParam, TResult>(KntCommandServiceBase<TParam, TResult> command) where TResult : ResultBase, new()
    {
        var validParam = command.ValidateParam();
        if (!validParam.IsValid)
        {
            var result = new TResult();
            result.AddErrorMessage("Invalid param. ");
            result.AddListErrorMessage(validParam.ListErrorMessage);
            Service.Logger?.LogTrace("Service Validate - {param} is not valid, errors: {errorMessage}", command.Param, validParam.ErrorMessage);
            return result;
        }

        Service.Logger?.LogTrace("Service Validate param - {param} is valid", command.Param);
        return await ExecuteCommand<TResult>(command);
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
            Service.Logger?.LogError(ex, "Service ExecuteCommand {command}", command.GetType());
            throw new KntServiceException($"KNote service error. ({command.GetType().Name}). ", ex);
        }
    }
      
}

