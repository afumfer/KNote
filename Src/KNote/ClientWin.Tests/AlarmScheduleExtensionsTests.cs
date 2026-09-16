using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

[TestClass]
public class AlarmScheduleExtensionsTests
{
    [TestMethod]
    public void ApplyAlarmControl_Standard_Deactivates()
    {
        var message = new KMessageDto { AlarmType = EnumAlarmType.Standard, AlarmActivated = true, AlarmDateTime = DateTime.Now.AddMinutes(-5) };

        message.ApplyAlarmControl();

        Assert.IsFalse(message.AlarmActivated);
    }

    [TestMethod]
    public void ApplyAlarmControl_Daily_AdvancesToNextFutureDay()
    {
        var firedAt = DateTime.Now.AddDays(-3).AddMinutes(-1);
        var message = new KMessageDto { AlarmType = EnumAlarmType.Daily, AlarmActivated = true, AlarmDateTime = firedAt };

        message.ApplyAlarmControl();

        Assert.IsTrue(message.AlarmDateTime > DateTime.Now);
        // Still the same time-of-day, just rolled forward in whole days.
        Assert.AreEqual(firedAt.TimeOfDay, message.AlarmDateTime.Value.TimeOfDay);
        Assert.IsTrue(message.AlarmActivated);
    }

    [TestMethod]
    public void ApplyAlarmControl_InMinutes_AdvancesByConfiguredStep()
    {
        var firedAt = DateTime.Now.AddMinutes(-25);
        var message = new KMessageDto { AlarmType = EnumAlarmType.InMinutes, AlarmMinutes = 10, AlarmActivated = true, AlarmDateTime = firedAt };

        message.ApplyAlarmControl();

        Assert.IsTrue(message.AlarmDateTime > DateTime.Now);
        Assert.AreEqual(0, ((message.AlarmDateTime.Value - firedAt).TotalMinutes) % 10);
    }

    [TestMethod]
    public void ApplyAlarmControl_UnrecognizedType_Deactivates()
    {
        var message = new KMessageDto { AlarmType = (EnumAlarmType)999, AlarmActivated = true, AlarmDateTime = DateTime.Now };

        message.ApplyAlarmControl();

        Assert.IsFalse(message.AlarmActivated);
    }
}
