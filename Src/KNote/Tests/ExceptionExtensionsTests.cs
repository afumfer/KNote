using KNote.Server.Helpers;
using KNote.Service.Core;

namespace KNote.Tests;

[TestClass]
public class ExceptionExtensionsTests
{
    [TestMethod]
    public void GetRootMessage_WithoutInnerException_ReturnsOwnMessage()
    {
        var ex = new Exception("boom");

        Assert.AreEqual("boom", ex.GetRootMessage());
    }

    [TestMethod]
    public void GetRootMessage_WrappedOnce_ReturnsInnerMessage()
    {
        var ex = new KntServiceException("KNote service error. (SomeCommand). ", new Exception("Email \"a@b.c\" is already in use"));

        Assert.AreEqual("Email \"a@b.c\" is already in use", ex.GetRootMessage());
    }

    [TestMethod]
    public void GetRootMessage_WrappedTwice_ReturnsInnermostMessage()
    {
        var ex = new KntServiceException("service wrapper", new InvalidOperationException("repository wrapper", new Exception("root cause")));

        Assert.AreEqual("root cause", ex.GetRootMessage());
    }

    [TestMethod]
    public void ToApiErrorMessage_PrefixesRootMessage()
    {
        var ex = new KntServiceException("KNote service error. (SomeCommand). ", new Exception("root cause"));

        Assert.AreEqual("Generic error: root cause", ex.ToApiErrorMessage());
    }
}
