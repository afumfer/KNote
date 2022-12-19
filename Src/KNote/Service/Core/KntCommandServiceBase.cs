using KNote.Model;
using KNote.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Service.Core;

public abstract class KntCommandServiceBase<TParam, TResult> : KntCommandServiceBase<TResult>
{
    public TParam Param { get; init; }

    public KntCommandServiceBase(IKntService service, TParam param) : base(service)
    {
        Param = param;
    }
}

public abstract class KntCommandServiceBase<TResult>
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

    public KntCommandServiceBase(IKntService service)
    {
        _service = service;

    }

    public virtual bool ValidateAuthorization()
    {
        return true;
    }

    public virtual bool ValidateParamn()
    {
        return true;
    }

    public abstract Task<TResult> Execute();
}
