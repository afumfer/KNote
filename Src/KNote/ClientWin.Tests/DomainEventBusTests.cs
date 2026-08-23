using KNote.ClientWin.Core;

namespace KNote.ClientWin.Tests;

[TestClass]
public class DomainEventBusTests
{
    private record TestMessage(string Text);
    private record OtherMessage(int Value);

    [TestMethod]
    public void Publish_NoSubscribers_DoesNotThrow()
    {
        var bus = new DomainEventBus();

        bus.Publish(new TestMessage("hello"));
    }

    [TestMethod]
    public void Publish_SingleSubscriber_ReceivesMessage()
    {
        var bus = new DomainEventBus();
        TestMessage? received = null;
        bus.Subscribe<TestMessage>(msg => received = msg);

        bus.Publish(new TestMessage("hello"));

        Assert.AreEqual("hello", received?.Text);
    }

    [TestMethod]
    public void Publish_MultipleSubscribersToSameType_AllReceiveMessage()
    {
        var bus = new DomainEventBus();
        var receivedCount = 0;
        bus.Subscribe<TestMessage>(_ => receivedCount++);
        bus.Subscribe<TestMessage>(_ => receivedCount++);

        bus.Publish(new TestMessage("hello"));

        Assert.AreEqual(2, receivedCount);
    }

    [TestMethod]
    public void Publish_OnlyNotifiesSubscribersOfThatExactMessageType()
    {
        var bus = new DomainEventBus();
        var testMessageReceived = false;
        var otherMessageReceived = false;
        bus.Subscribe<TestMessage>(_ => testMessageReceived = true);
        bus.Subscribe<OtherMessage>(_ => otherMessageReceived = true);

        bus.Publish(new TestMessage("hello"));

        Assert.IsTrue(testMessageReceived);
        Assert.IsFalse(otherMessageReceived);
    }

    [TestMethod]
    public void Unsubscribe_StopsReceivingFurtherMessages()
    {
        var bus = new DomainEventBus();
        var receivedCount = 0;
        void Handler(TestMessage _) => receivedCount++;
        bus.Subscribe<TestMessage>(Handler);

        bus.Publish(new TestMessage("first"));
        bus.Unsubscribe<TestMessage>(Handler);
        bus.Publish(new TestMessage("second"));

        Assert.AreEqual(1, receivedCount);
    }
}
