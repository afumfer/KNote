using System.Reflection;
using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Scripts saved in users' notes (KntScript / C# / Python / JavaScript) call into a few public members of
/// ClientWin by name. Their misspelled names were corrected, but the old ones must keep working.
/// </summary>
[TestClass]
public class ScriptApiCompatibilityTests
{
    [TestMethod]
    public void KNoteAIAssistantCtrl_MisspelledChatTextMessasges_StillReturnsTheSameTranscript()
    {
        var ctrl = new KNoteAIAssistantCtrl(new Store(new TestFactoryViews()));

#pragma warning disable CS0618
        Assert.AreSame(ctrl.ChatTextMessages, ctrl.ChatTextMessasges);
#pragma warning restore CS0618
    }

    [TestMethod]
    public void KNoteScriptLibrary_MisspelledGetKNoteManagmentCtrl_StillExistsAndIsMarkedObsolete()
    {
        var oldMethod = typeof(KNoteScriptLibrary).GetMethod("GetKNoteManagmentCtrl", Type.EmptyTypes);
        var newMethod = typeof(KNoteScriptLibrary).GetMethod("GetKNoteManagementCtrl", Type.EmptyTypes);

        Assert.IsNotNull(oldMethod);
        Assert.IsNotNull(newMethod);
        Assert.AreEqual(newMethod.ReturnType, oldMethod.ReturnType);
        Assert.IsNotNull(oldMethod.GetCustomAttribute<ObsoleteAttribute>());
    }
}
