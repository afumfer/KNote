using System;

using KNote.Model;

namespace KNote.Service.Core;

public enum CommandOutcome
{
    Succeeded,
    ValidationFailed,
    NotAuthorized,
    Faulted
}

/// <summary>
/// Fields common to CommandExecutingEventArgs/CommandExecutedEventArgs. Split into two types (rather
/// than one shared, partially-filled class) so subscribers never have to guess whether a null Result
/// means "not finished yet" or "the command produced no result".
/// </summary>
public abstract class CommandEventArgsBase : EventArgs
{
    public required Guid ExecutionId { get; init; }
    public required IKntService Service { get; init; }
    public required Type CommandType { get; init; }
    public string CommandName => CommandType.Name;

    /// <summary>
    /// The command's Param, boxed via KntCommandServiceBase&lt;TResult&gt;.ParamObject - null for
    /// commands with no param (KntCommandServiceBase&lt;TResult&gt; without a TParam).
    /// </summary>
    public object Param { get; init; }

    public required DateTime StartedAtUtc { get; init; }
}

public sealed class CommandExecutingEventArgs : CommandEventArgsBase
{
}

public sealed class CommandExecutedEventArgs : CommandEventArgsBase
{
    public required TimeSpan Duration { get; init; }
    public required CommandOutcome Outcome { get; init; }
    public bool Success => Outcome == CommandOutcome.Succeeded;

    /// <summary>Null when Outcome == Faulted (the command threw before producing a result).</summary>
    public ResultBase Result { get; init; }

    /// <summary>Null unless Outcome == Faulted.</summary>
    public Exception Exception { get; init; }
}
