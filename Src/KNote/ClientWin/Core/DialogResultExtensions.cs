using KNote.Model;

namespace KNote.ClientWin.Core;

public static class DialogResultExtensions
{
    // Pure translation of how a modal view was closed into the result a controller reports. Lives
    // here (not only on CtrlBase) so KntForm can do it without holding a reference to its controller.
    public static Result<EControllerResult> ToControllerResult(this DialogResult dialogResult)
    {
        var result = new Result<EControllerResult>();
        if (dialogResult == DialogResult.OK || dialogResult == DialogResult.Yes)
            result.Entity = EControllerResult.Executed;
        else
            result.Entity = EControllerResult.Canceled;
        return result;
    }
}
