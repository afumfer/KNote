using System.Net;
using System.Net.Sockets;
using KNote.Server.Local;
using Microsoft.Extensions.Configuration;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// KNote Local's data folder without a host: what the first start creates (folders, configuration with a secret of its
/// own encrypted, a SQLite repository, a free port), that it is never created again, and the secrets of its
/// configuration decrypted when it is read (or left empty, and said, when they can't be).
/// </summary>
[TestClass]
public class KntLocalDataTests
{
    private string _path = null!;

    [TestInitialize]
    public void Initialize() => _path = Path.Combine(Path.GetTempPath(), $"knote-local-{Guid.NewGuid():N}");

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_path))
            Directory.Delete(_path, true);
    }

    [TestMethod]
    public void EnsureCreated_FirstTime_FoldersAndConfiguration_WithItsOwnSecretEncrypted()
    {
        var localData = new KntLocalData(_path);
        var protector = new KntSecretProtector(localData.KeysFolder);

        Assert.IsTrue(localData.EnsureCreated(protector));

        foreach (var folder in new[] { localData.DataFolder, localData.ResourcesFolder, localData.KeysFolder, localData.LogsFolder })
            Assert.IsTrue(Directory.Exists(folder), folder);
        var config = localData.ReadConfig();
        var secret = config["AppSettings"]!["Secret"]!.GetValue<string>();
        Assert.IsTrue(KntSecretProtector.IsProtected(secret), "Encrypted in the file.");
        Assert.IsGreaterThanOrEqualTo(64, protector.Unprotect(secret).Length);
        Assert.IsTrue(Guid.TryParse(config["Local"]!["InstanceId"]!.GetValue<string>(), out _));
        StringAssert.StartsWith(config["Kestrel"]!["Endpoints"]!["Http"]!["Url"]!.GetValue<string>(), "http://127.0.0.1:");
        var repository = config["Repositories"]![0]!;
        Assert.AreEqual("Microsoft.Data.Sqlite", repository["Provider"]!.GetValue<string>());
        StringAssert.Contains(repository["ConnectionString"]!.GetValue<string>(), localData.DataFolder);
        StringAssert.StartsWith(repository["ResourcesContainerRootPath"]!.GetValue<string>(), localData.ResourcesFolder);
    }

    [TestMethod]
    public void EnsureCreated_Again_KeepsTheConfiguration()
    {
        var localData = new KntLocalData(_path);
        var protector = new KntSecretProtector(localData.KeysFolder);
        localData.EnsureCreated(protector);
        var first = File.ReadAllText(localData.ConfigFile);

        Assert.IsFalse(localData.EnsureCreated(protector));
        Assert.AreEqual(first, File.ReadAllText(localData.ConfigFile));
    }

    [TestMethod]
    public void EnsureCreated_ThePortInUse_TheNextFreeOne()
    {
        var used = new TcpListener(IPAddress.Loopback, 0);
        used.Start();
        try
        {
            var usedPort = ((IPEndPoint)used.LocalEndpoint).Port;
            var localData = new KntLocalData(_path);

            localData.EnsureCreated(new KntSecretProtector(localData.KeysFolder), usedPort);

            var url = localData.ReadConfig()["Kestrel"]!["Endpoints"]!["Http"]!["Url"]!.GetValue<string>();
            Assert.AreNotEqual($"http://127.0.0.1:{usedPort}", url);
        }
        finally
        {
            used.Stop();
        }
    }

    [TestMethod]
    public void Configuration_ItsProtectedValuesDecrypted_TheUnreadableOnesEmptyAndSaid()
    {
        var localData = new KntLocalData(_path);
        var protector = new KntSecretProtector(localData.KeysFolder);
        localData.EnsureCreated(protector);
        var config = localData.ReadConfig();
        config["Email"] = new System.Text.Json.Nodes.JsonObject
        {
            ["Password"] = protector.Protect("smtp-password"),
            ["Host"] = "smtp.example.com",
            ["Username"] = "protected:not-a-valid-protected-value"
        };
        localData.WriteConfig(config);

        var source = new KntProtectedJsonConfigurationSource { Path = localData.ConfigFile, Protector = protector };
        source.ResolveFileProvider();
        var configuration = new ConfigurationBuilder().Add(source).Build();
        var provider = configuration.Providers.OfType<KntProtectedJsonConfigurationProvider>().Single();

        Assert.AreEqual("smtp-password", configuration["Email:Password"]);
        Assert.AreEqual("smtp.example.com", configuration["Email:Host"]);
        Assert.AreEqual("", configuration["Email:Username"]);
        CollectionAssert.AreEqual(new[] { "Email:Username" }, provider.UnreadableKeys.ToArray());
    }

    [TestMethod]
    public void Protector_KeysOfThisFolder_SurviveANewInstance()
    {
        var localData = new KntLocalData(_path);
        Directory.CreateDirectory(localData.KeysFolder);
        var protectedValue = new KntSecretProtector(localData.KeysFolder).Protect("api-key");

        Assert.AreEqual("api-key", new KntSecretProtector(localData.KeysFolder).Unprotect(protectedValue), "As after reinstalling.");
        Assert.AreEqual("plain", new KntSecretProtector(localData.KeysFolder).Unprotect("plain"), "Typed by hand.");
    }
}
