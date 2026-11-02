using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Helpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// SemanticIndexJobs (Server, KNote Atlas), without a host and with runs held by the test: one queue for the Server and
/// one run at a time, one run per repository, the position in the queue, cancelling a queued or a running run, and a
/// failed run that does not stop the queue.
/// </summary>
[TestClass]
public class SemanticIndexJobsTests
{
    private sealed class HeldRuns
    {
        private readonly object _lock = new();
        public Dictionary<Guid, TaskCompletionSource<Result<SemanticIndexProgressDto>>> Releases { get; } = new();
        public List<Guid> Started { get; } = new();

        public async Task<Result<SemanticIndexProgressDto>> RunAsync(SemanticIndexJob job, IProgress<SemanticIndexProgressDto> progress,
            CancellationToken cancellationToken)
        {
            TaskCompletionSource<Result<SemanticIndexProgressDto>> release;
            lock (_lock)
                release = Releases[job.RepositoryId] = new(TaskCreationOptions.RunContinuationsAsynchronously);
            progress.Report(new SemanticIndexProgressDto { Phase = EnumSemanticIndexPhase.Indexing, NotesTotal = 10 });
            lock (_lock)
                Started.Add(job.RepositoryId);
            using (cancellationToken.Register(() => release.TrySetCanceled(cancellationToken)))
                return await release.Task;
        }

        public void Complete(Guid repositoryId, EnumSemanticIndexPhase phase = EnumSemanticIndexPhase.Completed)
        {
            lock (_lock)
                Releases[repositoryId].SetResult(new Result<SemanticIndexProgressDto>(new SemanticIndexProgressDto { Phase = phase, NotesDone = 10 }));
        }

        public bool HasStarted(Guid repositoryId)
        {
            lock (_lock)
                return Started.Contains(repositoryId);
        }
    }

    private static async Task WaitForAsync(Func<bool> condition)
    {
        for (var i = 0; i < 200 && !condition(); i++)
            await Task.Delay(25);
        Assert.IsTrue(condition(), "timed out");
    }

    private static (SemanticIndexJobs Jobs, HeldRuns Runs) Create()
    {
        var runs = new HeldRuns();
        return (new SemanticIndexJobs(runs.RunAsync, NullLogger<SemanticIndexJobs>.Instance), runs);
    }

    [TestMethod]
    public async Task OneRunAtATime_TheSecondRepositoryWaitsInTheQueue()
    {
        var (jobs, runs) = Create();
        await jobs.StartAsync(CancellationToken.None);
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        jobs.Enqueue(first, "First", "admin", false);
        await WaitForAsync(() => runs.HasStarted(first));
        var queued = jobs.Enqueue(second, "Second", "admin2", true);

        Assert.AreEqual(EnumSemanticIndexPhase.Queued, queued.Phase);
        Assert.AreEqual(1, queued.QueuePosition);
        Assert.AreEqual("admin2", queued.RequestedBy);
        Assert.IsTrue(queued.Rebuild);
        var running = jobs.GetProgress(first)!;
        Assert.AreEqual(EnumSemanticIndexPhase.Indexing, running.Phase);
        Assert.AreEqual(first, running.RepositoryId);
        Assert.AreEqual("First", running.RepositoryName);
        Assert.IsFalse(runs.HasStarted(second));

        runs.Complete(first);
        await WaitForAsync(() => runs.HasStarted(second));
        Assert.AreEqual(EnumSemanticIndexPhase.Completed, jobs.GetProgress(first)!.Phase);
        Assert.IsFalse(jobs.IsBusy(first));
        Assert.IsTrue(jobs.IsBusy(second));

        runs.Complete(second);
        await WaitForAsync(() => !jobs.IsBusy(second));
        await jobs.StopAsync(CancellationToken.None);
    }

    [TestMethod]
    public async Task ARepositoryHasOneRun_AskingAgainGivesTheOneThereIs()
    {
        var (jobs, runs) = Create();
        await jobs.StartAsync(CancellationToken.None);
        var repositoryId = Guid.NewGuid();

        jobs.Enqueue(repositoryId, "Repo", "admin", false);
        await WaitForAsync(() => runs.HasStarted(repositoryId));
        var again = jobs.Enqueue(repositoryId, "Repo", "other", true);

        Assert.AreEqual("admin", again.RequestedBy);
        Assert.IsFalse(again.Rebuild);
        Assert.AreEqual(1, runs.Started.Count);

        runs.Complete(repositoryId);
        await WaitForAsync(() => !jobs.IsBusy(repositoryId));
        var next = jobs.Enqueue(repositoryId, "Repo", "other", true);
        Assert.AreEqual("other", next.RequestedBy, "a finished run does not stop a new one");
        await WaitForAsync(() => runs.Started.Count == 2);
        runs.Complete(repositoryId);
        await jobs.StopAsync(CancellationToken.None);
    }

    [TestMethod]
    public async Task Cancel_AQueuedRun_LeavesTheQueue_AndARunningOne_Stops()
    {
        var (jobs, runs) = Create();
        await jobs.StartAsync(CancellationToken.None);
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        jobs.Enqueue(first, "First", "admin", false);
        await WaitForAsync(() => runs.HasStarted(first));
        jobs.Enqueue(second, "Second", "admin", false);

        Assert.IsTrue(jobs.Cancel(second));
        Assert.AreEqual(EnumSemanticIndexPhase.Canceled, jobs.GetProgress(second)!.Phase);
        Assert.IsTrue(jobs.Cancel(first));
        await WaitForAsync(() => jobs.GetProgress(first)!.Phase == EnumSemanticIndexPhase.Canceled);

        Assert.IsFalse(jobs.Cancel(first), "nothing left to cancel");
        Assert.IsFalse(jobs.Cancel(Guid.NewGuid()));
        await Task.Delay(100);
        Assert.IsFalse(runs.HasStarted(second));
        await jobs.StopAsync(CancellationToken.None);
    }

    [TestMethod]
    public async Task AFailedRun_DoesNotStopTheQueue()
    {
        var failing = Guid.NewGuid();
        var next = Guid.NewGuid();
        var started = new List<Guid>();
        var jobs = new SemanticIndexJobs((job, _, _) =>
        {
            lock (started)
                started.Add(job.RepositoryId);
            if (job.RepositoryId == failing)
                throw new InvalidOperationException("The database is gone.");
            return Task.FromResult(new Result<SemanticIndexProgressDto>(new SemanticIndexProgressDto { Phase = EnumSemanticIndexPhase.Completed }));
        }, NullLogger<SemanticIndexJobs>.Instance);

        jobs.Enqueue(failing, "Failing", "admin", false);
        jobs.Enqueue(next, "Next", "admin", false);
        await jobs.StartAsync(CancellationToken.None);
        await WaitForAsync(() => !jobs.IsBusy(next));

        var failed = jobs.GetProgress(failing)!;
        Assert.AreEqual(EnumSemanticIndexPhase.Failed, failed.Phase);
        Assert.AreEqual("The database is gone.", failed.Message);
        Assert.AreEqual(EnumSemanticIndexPhase.Completed, jobs.GetProgress(next)!.Phase);
        await jobs.StopAsync(CancellationToken.None);
    }
}
