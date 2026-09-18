using KNote.ClientWin.Core;
using KNote.ClientWin.Views;

namespace KNote.ClientWin.Tests;

[TestClass]
public class ViewConventionTests
{
    // KntForm is where the behavior shared by every view lives (ShowInfo, and the rest of the IViewBase
    // plumbing as it moves there), so a view that skips it silently loses that behavior.
    [TestMethod]
    public void EveryIViewBaseImplementation_DerivesFromKntForm()
    {
        var offenders = typeof(KntForm).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IViewBase).IsAssignableFrom(t))
            .Where(t => !typeof(KntForm).IsAssignableFrom(t))
            .Select(t => t.Name)
            .OrderBy(n => n)
            .ToList();

        Assert.AreEqual(0, offenders.Count, "Views not deriving from KntForm: " + string.Join(", ", offenders));
    }
}
