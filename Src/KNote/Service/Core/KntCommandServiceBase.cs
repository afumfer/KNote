using System.Threading.Tasks;
using KNote.Model;
using KNote.Repository;

namespace KNote.Service.Core;
public abstract class KntCommandSaveServiceBase<TParam, TResult> : KntCommandServiceBase<TParam, TResult> where TParam : ModelBase
{    
    public KntCommandSaveServiceBase(IKntService service, TParam param) : base(service, param)
    {
        
    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if(!Param.IsValid())
            result.AddErrorMessage(Param.GetErrorMessage());
        return result;
    }
}

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

    public string UserIdentityName
    {
        get { return Service.UserIdentityName; }
    }

    public KntCommandServiceBase(IKntService service)
    {
        _service = service;
    }

    public virtual Result ValidateAuthorization()
    {        
        return new Result();
    }

    public virtual Result ValidateParam()
    {
        return new Result();
    }

    public abstract Task<TResult> Execute();

}
