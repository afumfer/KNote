using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Tests;

[TestClass]
public class XmlConfigFileTests
{
    private string _dir = null!;
    private string _file = null!;

    [TestInitialize]
    public void Setup()
    {
        _dir = Path.Combine(Path.GetTempPath(), "KNoteXmlConfigFileTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
        _file = Path.Combine(_dir, "KNoteData.config");
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_dir, recursive: true);

    [TestMethod]
    public void Load_MissingFile_ReturnsNull()
    {
        var config = XmlConfigFile.Load<AppConfigV1>(_file, out var recovered);

        Assert.IsNull(config);
        Assert.IsFalse(recovered);
    }

    [TestMethod]
    public void Save_NewFile_RoundTripsAndLeavesNoTempOrBackup()
    {
        XmlConfigFile.Save(new AppConfigV1 { RunCounter = 7 }, _file);

        var config = XmlConfigFile.Load<AppConfigV1>(_file, out var recovered);

        Assert.AreEqual(7, config!.RunCounter);
        Assert.IsFalse(recovered);
        Assert.IsFalse(File.Exists(_file + ".tmp"));
        Assert.IsFalse(File.Exists(_file + XmlConfigFile.BackupExtension));
    }

    [TestMethod]
    public void SaveAndLoad_WorkForAnyXmlSerializableDocument()
    {
        XmlConfigFile.Save(new ServerCOMConfig { PortName = "COM5" }, _file);

        Assert.AreEqual("COM5", XmlConfigFile.Load<ServerCOMConfig>(_file, out _)!.PortName);
    }

    [TestMethod]
    public void Save_ExistingFile_KeepsPreviousVersionAsBackup()
    {
        XmlConfigFile.Save(new AppConfigV1 { RunCounter = 1 }, _file);
        XmlConfigFile.Save(new AppConfigV1 { RunCounter = 2 }, _file);

        Assert.AreEqual(2, XmlConfigFile.Load<AppConfigV1>(_file, out _)!.RunCounter);
        Assert.AreEqual(1, XmlConfigFile.Load<AppConfigV1>(_file + XmlConfigFile.BackupExtension, out _)!.RunCounter);
        Assert.IsFalse(File.Exists(_file + ".tmp"));
    }

    [TestMethod]
    public void Load_CorruptFileWithBackup_RecoversFromBackup()
    {
        XmlConfigFile.Save(new AppConfigV1 { RunCounter = 1 }, _file);
        XmlConfigFile.Save(new AppConfigV1 { RunCounter = 2 }, _file);
        File.WriteAllText(_file, "<AppConfigV1><RunCounter>");   // truncated write

        var config = XmlConfigFile.Load<AppConfigV1>(_file, out var recovered);

        Assert.IsTrue(recovered);
        Assert.AreEqual(1, config!.RunCounter);
    }

    [TestMethod]
    public void Load_CorruptFileWithoutBackup_Throws()
    {
        File.WriteAllText(_file, "not xml at all");

        Assert.ThrowsExactly<InvalidOperationException>(() => XmlConfigFile.Load<AppConfigV1>(_file, out _));
    }

    [TestMethod]
    public void Load_CorruptFileAndCorruptBackup_ThrowsOriginalError()
    {
        File.WriteAllText(_file, "not xml at all");
        File.WriteAllText(_file + XmlConfigFile.BackupExtension, "also broken");

        Assert.ThrowsExactly<InvalidOperationException>(() => XmlConfigFile.Load<AppConfigV1>(_file, out _));
    }

    // The frozen sample of a real (anonymized) V1 file. The restructuring must keep loading it without
    // losing any value, except the AppInfoAlarmRow rename below (a deliberately accepted exception).
    [TestMethod]
    public void Load_V1Fixture_LoadsEveryValue()
    {
        var config = XmlConfigFile.Load<AppConfigV1>(Path.Combine(AppContext.BaseDirectory, "Fixtures", "KNoteData.v1.config"), out _)!;

        Assert.AreEqual(3929, config.RunCounter);
        Assert.AreEqual(35, config.AlarmSeconds);
        Assert.AreEqual(1363, config.ManagmentWidth);
        Assert.IsTrue(config.AscendigOrderNotes);
        Assert.IsFalse(config.ShowHeaderPanel);
        Assert.AreEqual("Tags=45;InternalTags=92;Priority=52;NoteNumber=81", config.NotesListColumnWidths);
        Assert.AreEqual("http://chat.example.com/KNote/chathub", config.ChatHubUrl);
        Assert.AreEqual(2, config.RespositoryRefs.Count);
        Assert.IsTrue(config.RespositoryRefs[1].ResourceContentInDB);
        Assert.AreEqual("Mini Assistant", config.AssistantRespositoryRef.Alias);
        Assert.AreEqual("Personal respository", config.LastActiveRepositoryAlias);
        Assert.AreEqual(Guid.Parse("d577bc3e-64f8-417e-adeb-2b30b328b8d8"), config.LastActiveFolderId);
        Assert.AreEqual(3, config.AiProviderRefs.Count);
        Assert.AreEqual("test-anthropic-key", config.AiProviderRefs[1].ApiKey);
        Assert.AreEqual("Anthropic test", config.LastAiProviderAlias);
        Assert.AreEqual("COM7", config.ServerCOM.PortName);
        Assert.AreEqual(90, config.ServerCOM.RetroDelay);
        Assert.AreEqual("smtp.example.com", config.SmtpHost);
        Assert.AreEqual(465, config.SmtpPort);
        Assert.IsFalse(config.SmtpEnableSsl);
        Assert.AreEqual("test-smtp-password", config.SmtpPassword);
        Assert.AreEqual(2997, config.AppInfoAlarmsLocX);
        // The one accepted exception to "loses no value": AppInfoAlarmRow was renamed from
        // AppInfoAlarmRowConfig, and the old element name is deliberately not kept alive (see its doc
        // comment), so old rows come back empty rather than migrated.
        Assert.AreEqual(0, config.AppInfoAlarmsRows.Count);
    }
}
