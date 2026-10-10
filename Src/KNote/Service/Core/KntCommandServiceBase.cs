using System.Reflection;
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

    public override object ParamObject => Param;
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

    /// <summary>
    /// Checked by KntServiceBase.ExecuteCommand before Execute(). With Service.EnforceAuthorization on,
    /// the command runs only if its class is marked [KntAllowAnonymous], or declares [KntAuthorize] and
    /// the current user's role in this repository (Service.GetCurrentUserRoleAsync) is at least that one.
    /// A command declaring neither is refused: forgetting the attribute must not leave it open. Skipped inside
    /// a KntAuthorizationBypass scope, opened by a command that has already been authorized.
    /// </summary>
    public virtual async Task<Result> ValidateAuthorizationAsync()
    {
        var result = new Result();
        if (!Service.EnforceAuthorization || KntAuthorizationBypass.IsActive)
            return result;

        var commandType = GetType();
        if (commandType.GetCustomAttribute<KntAllowAnonymousAttribute>(inherit: false) != null)
            return result;

        var authorize = commandType.GetCustomAttribute<KntAuthorizeAttribute>(inherit: false);
        if (authorize == null)
        {
            result.AddErrorMessage($"The operation {commandType.Name} does not declare the role it requires.");
            return result;
        }

        var repositoryAlias = Service.RepositoryRef?.Alias;
        var role = await Service.GetCurrentUserRoleAsync();
        if (role == null)
            result.AddErrorMessage($"The user '{UserIdentityName}' is not registered in the repository '{repositoryAlias}'.");
        else if (role < authorize.MinimumRole)
            result.AddErrorMessage($"The user '{UserIdentityName}' needs the role '{authorize.MinimumRole}' in the repository '{repositoryAlias}' for this operation (current role: '{role}').");

        return result;
    }

    public virtual Result ValidateParam()
    {
        return new Result();
    }

    /// <summary>
    /// Boxed access to this command's Param, without every one of the ~80 concrete command classes
    /// needing to know about it - overridden by KntCommandServiceBase&lt;TParam, TResult&gt; to
    /// return its own Param. Used by KntServiceBase.ExecuteCommand to fill CommandEventArgsBase.Param
    /// uniformly regardless of which ExecuteCommand overload was called.
    /// </summary>
    public virtual object ParamObject => null;

    public abstract Task<TResult> Execute();

}
