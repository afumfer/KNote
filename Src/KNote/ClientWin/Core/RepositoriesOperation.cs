using KNote.Model;
using KNote.Service.Core;

namespace KNote.ClientWin.Core;

/// <summary>
/// One operation done in several linked repositories, one after the other, and what happened in each: for the
/// user's own account, which exists once in every repository (ChangePasswordCtrl, UserProfileCtrl). A repository
/// that fails doesn't stop the others.
/// </summary>
public class RepositoriesOperation<T> where T : ResultBase
{
    public List<(IKntService Service, T Result)> Done { get; } = new();

    public List<(IKntService Service, string Error)> Failed { get; } = new();

    public static async Task<RepositoriesOperation<T>> RunAsync(IEnumerable<IKntService> services, Func<IKntService, Task<T>> operation)
    {
        var result = new RepositoriesOperation<T>();

        foreach (var service in services)
        {
            try
            {
                var res = await operation(service);
                if (res.IsValid)
                    result.Done.Add((service, res));
                else
                    result.Failed.Add((service, res.ErrorMessage));
            }
            catch (Exception ex)
            {
                result.Failed.Add((service, CtrlBase.RootExceptionMessage(ex)));
            }
        }

        return result;
    }

    public T ResultOf(IKntService service) => Done.FirstOrDefault(d => d.Service == service).Result;

    // "Notes, Work" (the repositories where it was done).
    public string DoneText() => string.Join(", ", Done.Select(d => Name(d.Service)));

    // One line per repository where it failed, with why.
    public string FailedText() => string.Join(Environment.NewLine, Failed.Select(f => $"   {Name(f.Service)}: {f.Error?.Trim()}"));

    private static string Name(IKntService service) => service.RepositoryRef?.Name ?? "(repository)";
}
