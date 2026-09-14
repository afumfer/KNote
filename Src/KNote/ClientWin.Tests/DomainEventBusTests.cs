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

    // Regression coverage for DomainEventBus's underlying Dictionary<Type, List<Delegate>> not
    // being synchronized: Subscribe/Unsubscribe happen on the UI thread (e.g. opening/closing the
    // Monitor window) while Publish can be called from a background thread (a KntScript running on
    // its own Thread) - see DomainEventBus.cs's class-level comment. Unlike the WinForms UI-thread
    // marshaling fixed for MonitorForm.ShowInfo, DomainEventBus has no Control/STA dependency, so
    // this genuinely exercises real OS thread parallelism (via Task.Run), not just async
    // interleaving on one thread.
    [TestMethod]
    public async Task ConcurrentSubscribeUnsubscribePublish_FromManyThreads_DoesNotThrow()
    {
        var bus = new DomainEventBus();
        const int threadCount = 20;
        const int iterationsPerThread = 200;

        using var startGate = new ManualResetEventSlim(false);

        var tasks = Enumerable.Range(0, threadCount)
            .Select(threadIndex => Task.Run(() =>
            {
                void Handler(TestMessage _) { }

                startGate.Wait();

                for (var i = 0; i < iterationsPerThread; i++)
                {
                    // A mix of the three operations, so every thread is both a publisher and a
                    // subscriber/unsubscriber at the same time as every other thread.
                    bus.Subscribe<TestMessage>(Handler);
                    bus.Publish(new TestMessage($"thread {threadIndex}, iteration {i}"));
                    bus.Unsubscribe<TestMessage>(Handler);
                }
            }))
            .ToArray();

        startGate.Set();

        // Should not throw - a pre-fix run reliably throws (InvalidOperationException from the
        // Dictionary or the List, or occasionally a NullReferenceException/IndexOutOfRangeException
        // from a torn read) well before all threads finish.
        await Task.WhenAll(tasks);
    }
}
