using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Tests;

[TestClass]
public class OptionsModelTests
{
    private static readonly Guid WorkId = Guid.NewGuid();
    private static readonly Guid PersonalId = Guid.NewGuid();

    private static (AppUserSettings Settings, AppUserState State) NewConfig()
    {
        var settings = new AppUserSettings();
        settings.General.AlarmActivated = true;
        settings.General.AlarmSeconds = 35;
        settings.General.AutoSaveActivated = true;
        settings.General.AutoSaveSeconds = 60;
        settings.General.ColorMode = AppColorMode.Dark;
        settings.Security.AuthenticationMode = AppAuthenticationMode.Credentials;
        settings.Connectivity.ChatHub.Url = "http://chat.example.com/hub";
        settings.Connectivity.ChatHub.UserName = "jdoe";
        settings.Connectivity.ChatHub.Repositories.Add(new ChatHubRepositoryConfig { RepositoryId = WorkId, Name = "Work", Token = "token-1" });
        settings.Notifications.Email.Host = "smtp.example.com";
        settings.Notifications.Email.Port = 465;
        settings.Notifications.Email.EnableSsl = false;
        settings.Notifications.Email.FromAddress = "from@example.com";
        settings.Notifications.Email.FromDisplayName = "From";
        settings.Notifications.Email.Username = "user";
        settings.Notifications.Email.Password = "pwd";
        var state = new AppUserState();
        state.Session.ChatHubAutoConnectDisabled = true;
        return (settings, state);
    }

    [TestMethod]
    public void From_CopiesEverySettingAndStartsClean()
    {
        var (settings, state) = NewConfig();

        var model = OptionsModel.From(settings, state);

        Assert.IsTrue(model.AlarmActivated);
        Assert.AreEqual(35, model.AlarmSeconds);
        Assert.IsTrue(model.AutoSaveActivated);
        Assert.AreEqual(60, model.AutoSaveSeconds);
        Assert.AreEqual(AppColorMode.Dark, model.ColorMode);
        Assert.AreEqual(AppAuthenticationMode.Credentials, model.AuthenticationMode);
        Assert.AreEqual("http://chat.example.com/hub", model.ChatHubUrl);
        Assert.AreEqual("jdoe", model.ChatHubUserName);
        Assert.AreEqual("token-1", model.ChatHubRepositories.Single().Token);
        Assert.AreEqual(WorkId, model.ChatHubRepositories.Single().RepositoryId);
        Assert.AreNotSame(settings.Connectivity.ChatHub.Repositories[0], model.ChatHubRepositories[0], "A copy: editing the model doesn't change the settings.");
        Assert.IsTrue(model.ChatHubAutoConnectDisabled);
        Assert.AreEqual("smtp.example.com", model.SmtpHost);
        Assert.AreEqual(465, model.SmtpPort);
        Assert.IsFalse(model.SmtpEnableSsl);
        Assert.AreEqual("from@example.com", model.SmtpFromAddress);
        Assert.AreEqual("From", model.SmtpFromDisplayName);
        Assert.AreEqual("user", model.SmtpUsername);
        Assert.AreEqual("pwd", model.SmtpPassword);
        Assert.IsFalse(model.IsDirty(), "an untouched model must not look edited");
    }

    [TestMethod]
    public void ApplyTo_WritesEachValueToItsOwnSettingOrStateField()
    {
        var (settings, state) = NewConfig();
        var model = OptionsModel.From(settings, state);
        model.AlarmActivated = false;
        model.AlarmSeconds = 40;
        model.AutoSaveActivated = false;
        model.AutoSaveSeconds = 90;
        model.ColorMode = AppColorMode.System;
        model.AuthenticationMode = AppAuthenticationMode.Windows;
        model.ChatHubUrl = "http://other/hub";
        model.ChatHubUserName = "ana";
        model.ChatHubRepositories = new() { new ChatHubRepositoryConfig { RepositoryId = PersonalId, Name = "Personal", Token = "token-2" } };
        model.ChatHubAutoConnectDisabled = false;
        model.SmtpHost = "smtp.other.com";
        model.SmtpPort = 25;
        model.SmtpEnableSsl = true;
        model.SmtpFromAddress = "a@other.com";
        model.SmtpFromDisplayName = "A";
        model.SmtpUsername = "u2";
        model.SmtpPassword = "p2";
        Assert.IsTrue(model.IsDirty());

        model.ApplyTo(settings, state);

        Assert.IsFalse(settings.General.AlarmActivated);
        Assert.AreEqual(40, settings.General.AlarmSeconds);
        Assert.IsFalse(settings.General.AutoSaveActivated);
        Assert.AreEqual(90, settings.General.AutoSaveSeconds);
        Assert.AreEqual(AppColorMode.System, settings.General.ColorMode);
        Assert.AreEqual(AppAuthenticationMode.Windows, settings.Security.AuthenticationMode);
        Assert.AreEqual("http://other/hub", settings.Connectivity.ChatHub.Url);
        Assert.AreEqual("ana", settings.Connectivity.ChatHub.UserName);
        Assert.AreEqual("token-2", settings.Connectivity.ChatHub.Token);
        Assert.AreEqual("Personal", settings.Connectivity.ChatHub.Current().Name);
        Assert.IsFalse(state.Session.ChatHubAutoConnectDisabled);
        var email = settings.Notifications.Email;
        Assert.AreEqual("smtp.other.com", email.Host);
        Assert.AreEqual(25, email.Port);
        Assert.IsTrue(email.EnableSsl);
        Assert.AreEqual("a@other.com", email.FromAddress);
        Assert.AreEqual("A", email.FromDisplayName);
        Assert.AreEqual("u2", email.Username);
        Assert.AreEqual("p2", email.Password);
    }

    [TestMethod]
    public void EditingTheModel_DoesNotTouchTheRealConfigurationUntilApplied()
    {
        var (settings, state) = NewConfig();
        var model = OptionsModel.From(settings, state);

        model.AlarmSeconds = 99;
        model.SmtpPassword = "changed";

        Assert.AreEqual(35, settings.General.AlarmSeconds);
        Assert.AreEqual("pwd", settings.Notifications.Email.Password);
    }

    [TestMethod]
    [DataRow(29, 60, false)]
    [DataRow(30, 60, true)]
    [DataRow(300, 600, true)]
    [DataRow(301, 60, false)]
    [DataRow(35, 59, false)]
    [DataRow(35, 601, false)]
    public void Validate_EnforcesAlarmAndAutoSaveRanges(int alarmSeconds, int autoSaveSeconds, bool valid)
    {
        var model = new OptionsModel { AlarmSeconds = alarmSeconds, AutoSaveSeconds = autoSaveSeconds };

        Assert.AreEqual(valid, string.IsNullOrEmpty(model.GetErrorMessage(false)));
    }
}
