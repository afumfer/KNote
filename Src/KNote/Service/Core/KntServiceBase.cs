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
    // actually executing the command is already caught, logged, and wrapped by ExecuteCommandTracked below.
    // Wrapping it again here used to double-wrap the exception, hiding the real cause behind two layers of
    // generic "KNote service error" messages.
    public async Task<TResult> ExecuteCommand<TParam, TResult>(KntCommandServiceBase<TParam, TResult> command) where TResult : ResultBase, new()
    {
        return await ExecuteCommandTracked(command, async () =>
        {
            var validParam = command.ValidateParam();
            if (!validParam.IsValid)
            {
                var result = new TResult();
                result.AddErrorMessage("Invalid param. ");
                result.AddListErrorMessage(validParam.ListErrorMessage);
                Service.Logger?.LogTrace("Service Validate - {param} is not valid, errors: {errorMessage}", command.Param, validParam.ErrorMessage);
                return (result, CommandOutcome.ValidationFailed);
            }

            Service.Logger?.LogTrace("Service Validate param - {param} is valid", command.Param);
            return await ExecuteCommandCore(command);
        });
    }

    public async Task<TResult> ExecuteCommand<TResult>(KntCommandServiceBase<TResult> command) where TResult : ResultBase, new()
    {
        return await ExecuteCommandTracked(command, () => ExecuteCommandCore(command));
    }

    // Wraps the outer ExecuteCommand overloads so CommandExecuting/CommandExecuted fire exactly once
    // per public call, correlated by ExecutionId, regardless of which of the two public overloads was
    // invoked and regardless of the exit path (success, validation failure, authorization failure, or
    // exception).
    private async Task<TResult> ExecuteCommandTracked<TResult>(KntCommandServiceBase<TResult> command, Func<Task<(TResult Result, CommandOutcome Outcome)>> body) where TResult : ResultBase, new()
    {
        var executionId = Guid.NewGuid();
        var startedAtUtc = DateTime.UtcNow;

        Service.NotifyCommandExecuting(new CommandExecutingEventArgs
        {
            ExecutionId = executionId,
            Service = Service,
            CommandType = command.GetType(),
            Param = command.ParamObject,
            StartedAtUtc = startedAtUtc
        });

        try
        {
            var (result, outcome) = await body();

            Service.NotifyCommandExecuted(new CommandExecutedEventArgs
            {
                ExecutionId = executionId,
                Service = Service,
                CommandType = command.GetType(),
                Param = command.ParamObject,
                StartedAtUtc = startedAtUtc,
                Duration = DateTime.UtcNow - startedAtUtc,
                Outcome = outcome,
                Result = result
            });

            return result;
        }
        catch (Exception ex)
        {
            Service.Logger?.LogError(ex, "Service ExecuteCommand {command}", command.GetType());

            Service.NotifyCommandExecuted(new CommandExecutedEventArgs
            {
                ExecutionId = executionId,
                Service = Service,
                CommandType = command.GetType(),
                Param = command.ParamObject,
                StartedAtUtc = startedAtUtc,
                Duration = DateTime.UtcNow - startedAtUtc,
                Outcome = CommandOutcome.Faulted,
                Exception = ex
            });

            throw new KntServiceException($"KNote service error. ({command.GetType().Name}). ", ex);
        }
    }

    private async Task<(TResult Result, CommandOutcome Outcome)> ExecuteCommandCore<TResult>(KntCommandServiceBase<TResult> command) where TResult : ResultBase, new()
    {
        var validAuthorization = command.ValidateAuthorization();
        if (validAuthorization.IsValid)
        {
            Service.Logger?.LogTrace("Service validated authorization for {command}", command.GetType());
            var result = await command.Execute();
            Service.Logger?.LogTrace("Service ExecutedCommand {command}", command.GetType());
            return (result, CommandOutcome.Succeeded);
        }
        else
        {
            var result = new TResult();
            result.AddErrorMessage("Not authorized. ");
            result.AddListErrorMessage(validAuthorization.ListErrorMessage);
            Service.Logger?.LogTrace("Service authorization is not valid for {command}", command.GetType());
            return (result, CommandOutcome.NotAuthorized);
        }
    }

}

