using KNote.Repository;
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

    public string UserIdentityName
    {
        get { return Service.UserIdentityName; }
    }

    public KntCommandServiceBase(IKntService service)
    {
        _service = service;

    }

    public virtual bool ValidateAuthorization()
    {
        // TODO: Check generic authorization for UserIdentityName
        return true;
    }

    public virtual bool ValidateParam()
    {
        return true;
    }

    public abstract Task<TResult> Execute();

}
