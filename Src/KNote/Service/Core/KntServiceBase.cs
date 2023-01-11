using KNote.Model;
using KNote.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Service.Core;

public abstract class KntServiceBase : DomainActionBase
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
                result = await ExecuteCommand<TResult>(command);
            else
            {
                result = new TResult();
                result.AddErrorMessage("Invalid param. ");
                result.AddListErrorMessage(validParam.ListErrorMessage);
                return result;
            }
            
            return result;
        }
        catch (Exception ex)
        {
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
                // TODO: other pre execute methods (log, events, ...)
                
                result = await command.Execute();
                
                // TODO: other post execute methods (log, events, ...)
            }
            else
            {
                result = new TResult();
                result.AddErrorMessage("Not authorized. ");
                result.AddListErrorMessage(validAuthorization.ListErrorMessage);
            }           
        
            return result;
        }
        catch (Exception ex)
        {
            throw new KntServiceException($"KNote service error.  ({MethodBase.GetCurrentMethod().DeclaringType}). ", ex);
        }
        
    }
}
