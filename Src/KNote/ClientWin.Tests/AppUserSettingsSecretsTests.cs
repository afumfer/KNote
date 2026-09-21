using System.Xml.Serialization;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;

namespace KNote.ClientWin.Tests;

[TestClass]
public class AppUserSettingsSecretsTests
{
    private const string SqlAuthConnectionString = "Server=db;Database=KNote;User Id=sa;Password=s3cr3t;Encrypt=false";

    private static AppUserSettings NewSettings()
    {
        var settings = new AppUserSettings();
        settings.Notifications.Email.Host = "smtp.example.com";
        settings.Notifications.Email.Password = "smtp-pwd";
        settings.Ai.Providers.Add(new AiProviderRef { Alias = "OpenAI", Provider = "OpenAI", Model = "m", ApiKey = "key-1" });
        settings.Ai.Providers.Add(new AiProviderRef { Alias = "Ollama", Provider = "Ollama", Model = "m", Host = "http://localhost:11434" });
        settings.Repositories.Items.Add(new RepositoryRef { Alias = "Sqlite", ConnectionString = @"Data Source=C:\KaNote\Data\a.db" });
        settings.Repositories.Items.Add(new RepositoryRef { Alias = "Sql", ConnectionString = SqlAuthConnectionString });
        settings.Repositories.Assistant = new RepositoryRef { Alias = "Assistant", ConnectionString = SqlAuthConnectionString };
        return settings;
    }

    private static string ToXml(AppUserSettings settings)
    {
        using var writer = new StringWriter();
        new XmlSerializer(typeof(AppUserSettings)).Serialize(writer, settings);
        return writer.ToString();
    }

    [TestMethod]
    public void Protect_EncryptsEverySecret_AndLeavesTheInputUntouched()
    {
        var secrets = new AppUserSettingsSecrets(new FakeSecretProtector());
        var settings = NewSettings();

        var protectedSettings = secrets.Protect(settings);

        Assert.AreEqual("fake:dwp-ptms", protectedSettings.Notifications.Email.Password);
        Assert.AreEqual("fake:1-yek", protectedSettings.Ai.Providers[0].ApiKey);
        Assert.IsTrue(protectedSettings.Repositories.Items[1].ConnectionString.StartsWith("fake:"));
        Assert.IsTrue(protectedSettings.Repositories.Assistant.ConnectionString.StartsWith("fake:"));
        Assert.AreEqual("smtp-pwd", settings.Notifications.Email.Password);
        Assert.AreEqual("key-1", settings.Ai.Providers[0].ApiKey);
        Assert.AreEqual(SqlAuthConnectionString, settings.Repositories.Items[1].ConnectionString);
    }

    [TestMethod]
    public void Protect_LeavesNonSecretsReadable()
    {
        var protectedSettings = new AppUserSettingsSecrets(new FakeSecretProtector()).Protect(NewSettings());

        Assert.AreEqual("smtp.example.com", protectedSettings.Notifications.Email.Host);
        Assert.AreEqual("http://localhost:11434", protectedSettings.Ai.Providers[1].Host);
        Assert.IsTrue(string.IsNullOrEmpty(protectedSettings.Ai.Providers[1].ApiKey));
        // No credentials inside: stays readable (SQLite / Trusted_Connection strings are not secrets).
        Assert.AreEqual(@"Data Source=C:\KaNote\Data\a.db", protectedSettings.Repositories.Items[0].ConnectionString);
    }

    [TestMethod]
    public void Protect_ThenSerialize_LeavesNoPlainTextSecretInTheFile()
    {
        var xml = ToXml(new AppUserSettingsSecrets(new FakeSecretProtector()).Protect(NewSettings()));

        foreach (var secret in new[] { "smtp-pwd", "key-1", "s3cr3t" })
            Assert.IsFalse(xml.Contains(secret), $"'{secret}' must not appear in clear text");
    }

    [TestMethod]
    public void ProtectThenUnprotect_RestoresTheOriginalSettings()
    {
        var secrets = new AppUserSettingsSecrets(new FakeSecretProtector());
        var original = NewSettings();
        var loaded = (AppUserSettings)new XmlSerializer(typeof(AppUserSettings))
            .Deserialize(new StringReader(ToXml(secrets.Protect(original))))!;

        var failed = secrets.Unprotect(loaded);

        Assert.AreEqual(0, failed.Count);
        Assert.AreEqual(ToXml(original), ToXml(loaded));
    }

    [TestMethod]
    public void Unprotect_PlainTextSecrets_AreKeptAsTheyAre()
    {
        // What a migrated V1 config (or a hand-edited file) looks like: secrets in clear text.
        var settings = NewSettings();

        var failed = new AppUserSettingsSecrets(new FakeSecretProtector()).Unprotect(settings);

        Assert.AreEqual(0, failed.Count);
        Assert.AreEqual("smtp-pwd", settings.Notifications.Email.Password);
        Assert.AreEqual("key-1", settings.Ai.Providers[0].ApiKey);
        Assert.AreEqual(SqlAuthConnectionString, settings.Repositories.Items[1].ConnectionString);
    }

    [TestMethod]
    public void Unprotect_SecretThatCannotBeDecrypted_IsClearedAndReported_WithoutAffectingTheRest()
    {
        var protector = new FakeSecretProtector();
        var secrets = new AppUserSettingsSecrets(protector);
        var settings = secrets.Protect(NewSettings());
        protector.Undecryptable.Add(settings.Ai.Providers[0].ApiKey);   // e.g. file copied from another user

        var failed = secrets.Unprotect(settings);

        Assert.AreEqual(1, failed.Count);
        StringAssert.Contains(failed[0], "OpenAI");
        Assert.IsNull(settings.Ai.Providers[0].ApiKey);
        Assert.AreEqual("smtp-pwd", settings.Notifications.Email.Password);
        Assert.AreEqual(SqlAuthConnectionString, settings.Repositories.Items[1].ConnectionString);
    }

    [TestMethod]
    public void Constructor_NullProtector_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new AppUserSettingsSecrets(null!));
    }

    // --- DpapiSecretProtector: real Windows DPAPI (current user) ---

    [TestMethod]
    public void Dpapi_RoundTrip_AndStoredFormatIsNotThePlainText()
    {
        var protector = new DpapiSecretProtector();

        var stored = protector.Protect("my secret ñ €");

        Assert.IsTrue(stored.StartsWith(DpapiSecretProtector.Prefix));
        Assert.IsFalse(stored.Contains("secret"));
        Assert.IsTrue(protector.TryUnprotect(stored, out var plain));
        Assert.AreEqual("my secret ñ €", plain);
    }

    [TestMethod]
    public void Dpapi_NullEmptyAndPlainValues_PassThrough()
    {
        var protector = new DpapiSecretProtector();

        Assert.IsNull(protector.Protect(null!));
        Assert.AreEqual("", protector.Protect(""));
        Assert.IsTrue(protector.TryUnprotect("legacy plain text", out var plain));
        Assert.AreEqual("legacy plain text", plain);
        Assert.IsTrue(protector.TryUnprotect(null!, out var nothing));
        Assert.IsNull(nothing);
    }

    [TestMethod]
    public void Dpapi_CorruptedOrForeignValue_ReturnsFalseInsteadOfThrowing()
    {
        var protector = new DpapiSecretProtector();

        Assert.IsFalse(protector.TryUnprotect("dpapi:this-is-not-base64!", out var a));
        Assert.IsNull(a);
        Assert.IsFalse(protector.TryUnprotect("dpapi:" + Convert.ToBase64String(new byte[] { 1, 2, 3, 4 }), out var b));
        Assert.IsNull(b);
    }

    [TestMethod]
    public void Dpapi_WithTheCodec_RoundTripsRealSettings()
    {
        var secrets = new AppUserSettingsSecrets(new DpapiSecretProtector());
        var original = NewSettings();

        var stored = secrets.Protect(original);
        Assert.IsTrue(stored.Notifications.Email.Password.StartsWith(DpapiSecretProtector.Prefix));
        var failed = secrets.Unprotect(stored);

        Assert.AreEqual(0, failed.Count);
        Assert.AreEqual(ToXml(original), ToXml(stored));
    }

    // --- ConnectionStringSecrets ---

    [TestMethod]
    [DataRow("Server=db;Password=x", true)]
    [DataRow("Server=db;pwd=x", true)]
    [DataRow("Server=db; PASSWORD = x ;Encrypt=false", true)]
    [DataRow(@"Data Source=C:\KaNote\Data\a.db", false)]
    [DataRow("Server=.;Trusted_Connection=True;Encrypt=false", false)]
    [DataRow("", false)]
    [DataRow(null, false)]
    public void ConnectionStringSecrets_HasPassword(string? connectionString, bool expected)
        => Assert.AreEqual(expected, ConnectionStringSecrets.HasPassword(connectionString!));

    [TestMethod]
    [DataRow("Server=db;Password=s3cr3t;Encrypt=false", "Server=db;Password=;Encrypt=false")]
    [DataRow("Server=db;Pwd=s3cr3t", "Server=db;Pwd=")]
    [DataRow("Server=db;Password=\"se;cret\";Encrypt=false", "Server=db;Password=;Encrypt=false")]
    [DataRow("Server=db;Password='se;cret';Encrypt=false", "Server=db;Password=;Encrypt=false")]
    [DataRow("Server=.;Trusted_Connection=True", "Server=.;Trusted_Connection=True")]
    public void ConnectionStringSecrets_WithoutPassword(string connectionString, string expected)
        => Assert.AreEqual(expected, ConnectionStringSecrets.WithoutPassword(connectionString));
}
