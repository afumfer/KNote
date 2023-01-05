using KNote.Model;
using KNote.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
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
        TResult res;

        try
        {
            if(command.ValidateParam())
                res = await ExecuteCommand<TResult>(command);
            else
            {
                res = new TResult();
                res.AddErrorMessage("Invalid param");
                return res;
            }
        }
        catch (Exception ex)
        {            
            res = new TResult();
            AddExecptionsMessagesToResult(ex, res);
            throw new KntServiceException(res.ErrorMessage, ex);
        }
        return res;
    }

    public async Task<TResult> ExecuteCommand<TResult>(KntCommandServiceBase<TResult> command) where TResult : ResultBase, new()
    {
        TResult res; // = new TResult();

        try            
        {
            if (command.ValidateAuthorization())
            {                
                // TODO: other pre execute methods (log, events, ...)
                
                res = await command.Execute();
                
                // TODO: other post execute methods (log, events, ...)
            }
            else
            {
                res = new TResult();
                res.AddErrorMessage("Not authorized.");                
            }           
        }
        catch (Exception ex)
        {            
            res = new TResult();
            AddExecptionsMessagesToResult(ex, res);
            throw new KntServiceException(res.ErrorMessage, ex);
        }
        
        return res;
    }
}
