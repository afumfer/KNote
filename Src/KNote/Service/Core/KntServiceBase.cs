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
        TResult res = new TResult(); 

        try
        {
            // TODO: valid param

            // TODO: valid authorization 

            // TODO: other pre execute methods (log, events, ...)

            res = await command.Execute();

            // TODO: other post execute methods (log, events, ...)

        }
        catch (Exception ex)
        {
            if (res == null)
                res = new TResult();
            AddExecptionsMessagesToResult(ex, res);

        }
        return res;
    }

    public async Task<TResult> ExecuteCommand<TResult>(KntCommandServiceBase<TResult> command) where TResult : ResultBase, new()
    {
        TResult res = new TResult();

        try
        {            
            // TODO: valid authorization 

            // TODO: other pre execute methods (log, events, ...)

            res = await command.Execute();

            // TODO: other post execute methods (log, events, ...)

        }
        catch (Exception ex)
        {
            if (res == null)
                res = new TResult();
            AddExecptionsMessagesToResult(ex, res);

        }
        return res;
    }


}
