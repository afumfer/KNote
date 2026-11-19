using KNote.ClientWin.Core;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Before any test: ClientWin's user data folder (AppUserDataPath) is a temporary folder of this run, so a Store.SaveConfig() reached by
/// a test (a ctrl whose SaveSettings was not replaced) writes there and never over the user's real KNoteData.config and KNoteState.config
/// in %LocalAppData%\KNote (it happened on 2026-11-19 and on 2026-11-08).
/// </summary>
[TestClass]
public class TestAssemblySetup
{
    private static string? _folder;

    [AssemblyInitialize]
    public static void Initialize(TestContext context)
    {
        _folder = Path.Combine(Path.GetTempPath(), "KNote.ClientWin.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
        Environment.SetEnvironmentVariable(AppUserDataPath.OverrideVariable, _folder);
        if (!string.Equals(AppUserDataPath.Directory, _folder, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"AppUserDataPath was read before the tests started: {AppUserDataPath.Directory}");
    }

    [TestMethod]
    public void TheUserDataFolder_IsNotTheRealOne()
    {
        StringAssert.StartsWith(AppUserDataPath.ConfigFile, Path.GetTempPath(), "a SaveConfig() of a test would overwrite the user's configuration");
        var store = new Store(new Fakes.TestFactoryViews());
        store.SaveConfig();
        Assert.IsTrue(File.Exists(Path.Combine(_folder!, "KNoteData.config")));
    }

    [AssemblyCleanup]
    public static void Cleanup()
    {
        try
        {
            if (_folder != null && Directory.Exists(_folder))
                Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
        }
    }
}
