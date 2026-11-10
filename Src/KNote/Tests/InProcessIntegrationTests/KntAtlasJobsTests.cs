using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Helpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// KntAtlasJobs (Server, KNote Atlas, D11), without a host and with runs held by the test: one queue for the Server and
/// one run at a time, one run per repository and kind (the semantic index, «Process pending resources»), the position in
/// the queue, cancelling a queued or a running run, a failed run that does not stop the queue, and the update of the index
/// queued after a run of resources that described something.
/// </summary>
[TestClass]
public class KntAtlasJobsTests
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

    private static (KntAtlasJobs Jobs, HeldRuns Runs) Create()
    {
        var runs = new HeldRuns();
        return (new KntAtlasJobs(runs.RunAsync, NoResources, NullLogger<KntAtlasJobs>.Instance), runs);
    }

    private static Task<Result<ResourceProcessingProgressDto>> NoResources(ResourceProcessingJob job, IProgress<ResourceProcessingProgressDto> progress,
        CancellationToken cancellationToken) =>
        Task.FromResult(new Result<ResourceProcessingProgressDto>(new ResourceProcessingProgressDto { Phase = EnumResourceProcessingPhase.Completed }));

    [TestMethod]
    public async Task OneRunAtATime_TheSecondRepositoryWaitsInTheQueue()
    {
        var (jobs, runs) = Create();
        await jobs.StartAsync(CancellationToken.None);
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        jobs.EnqueueIndex(first, "First", "admin", false);
        await WaitForAsync(() => runs.HasStarted(first));
        var queued = jobs.EnqueueIndex(second, "Second", "admin2", true);

        Assert.AreEqual(EnumSemanticIndexPhase.Queued, queued.Phase);
        Assert.AreEqual(1, queued.QueuePosition);
        Assert.AreEqual("admin2", queued.RequestedBy);
        Assert.IsTrue(queued.Rebuild);
        var running = jobs.GetIndexProgress(first)!;
        Assert.AreEqual(EnumSemanticIndexPhase.Indexing, running.Phase);
        Assert.AreEqual(first, running.RepositoryId);
        Assert.AreEqual("First", running.RepositoryName);
        Assert.IsFalse(runs.HasStarted(second));

        runs.Complete(first);
        await WaitForAsync(() => runs.HasStarted(second));
        Assert.AreEqual(EnumSemanticIndexPhase.Completed, jobs.GetIndexProgress(first)!.Phase);
        Assert.IsFalse(jobs.IsIndexBusy(first));
        Assert.IsTrue(jobs.IsIndexBusy(second));

        runs.Complete(second);
        await WaitForAsync(() => !jobs.IsIndexBusy(second));
        await jobs.StopAsync(CancellationToken.None);
    }

    [TestMethod]
    public async Task ARepositoryHasOneRun_AskingAgainGivesTheOneThereIs()
    {
        var (jobs, runs) = Create();
        await jobs.StartAsync(CancellationToken.None);
        var repositoryId = Guid.NewGuid();

        jobs.EnqueueIndex(repositoryId, "Repo", "admin", false);
        await WaitForAsync(() => runs.HasStarted(repositoryId));
        var again = jobs.EnqueueIndex(repositoryId, "Repo", "other", true);

        Assert.AreEqual("admin", again.RequestedBy);
        Assert.IsFalse(again.Rebuild);
        Assert.AreEqual(1, runs.Started.Count);

        runs.Complete(repositoryId);
        await WaitForAsync(() => !jobs.IsIndexBusy(repositoryId));
        var next = jobs.EnqueueIndex(repositoryId, "Repo", "other", true);
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
        jobs.EnqueueIndex(first, "First", "admin", false);
        await WaitForAsync(() => runs.HasStarted(first));
        jobs.EnqueueIndex(second, "Second", "admin", false);

        Assert.IsTrue(jobs.CancelIndex(second));
        Assert.AreEqual(EnumSemanticIndexPhase.Canceled, jobs.GetIndexProgress(second)!.Phase);
        Assert.IsTrue(jobs.CancelIndex(first));
        await WaitForAsync(() => jobs.GetIndexProgress(first)!.Phase == EnumSemanticIndexPhase.Canceled);

        Assert.IsFalse(jobs.CancelIndex(first), "nothing left to cancel");
        Assert.IsFalse(jobs.CancelIndex(Guid.NewGuid()));
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
        var jobs = new KntAtlasJobs((job, _, _) =>
        {
            lock (started)
                started.Add(job.RepositoryId);
            if (job.RepositoryId == failing)
                throw new InvalidOperationException("The database is gone.");
            return Task.FromResult(new Result<SemanticIndexProgressDto>(new SemanticIndexProgressDto { Phase = EnumSemanticIndexPhase.Completed }));
        }, NoResources, NullLogger<KntAtlasJobs>.Instance);

        jobs.EnqueueIndex(failing, "Failing", "admin", false);
        jobs.EnqueueIndex(next, "Next", "admin", false);
        await jobs.StartAsync(CancellationToken.None);
        await WaitForAsync(() => !jobs.IsIndexBusy(next));

        var failed = jobs.GetIndexProgress(failing)!;
        Assert.AreEqual(EnumSemanticIndexPhase.Failed, failed.Phase);
        Assert.AreEqual("The database is gone.", failed.Message);
        Assert.AreEqual(EnumSemanticIndexPhase.Completed, jobs.GetIndexProgress(next)!.Phase);
        await jobs.StopAsync(CancellationToken.None);
    }

    [TestMethod]
    public async Task TheIndexAndTheResourcesOfARepository_AreTwoRuns_InTheSameQueue()
    {
        var (_, runs) = Create();
        var resourcesStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var jobs = new KntAtlasJobs(runs.RunAsync, (job, progress, _) =>
        {
            resourcesStarted.TrySetResult();
            return Task.FromResult(new Result<ResourceProcessingProgressDto>(new ResourceProcessingProgressDto
            {
                Phase = EnumResourceProcessingPhase.Completed, PhotosTotal = 2, PhotosDone = 2, PhotosDescribed = 2
            }));
        }, NullLogger<KntAtlasJobs>.Instance);
        await jobs.StartAsync(CancellationToken.None);
        var repositoryId = Guid.NewGuid();
        var provider = new AiProviderRef { Alias = "Local", Provider = EnumAiProvider.Ollama, Model = "qwen3.5:9b" };

        jobs.EnqueueIndex(repositoryId, "Repo", "admin", false);
        await WaitForAsync(() => runs.HasStarted(repositoryId));
        var queued = jobs.EnqueueResources(repositoryId, "Repo", "admin", provider, new ResourceProcessingOptionsDto());

        Assert.AreEqual(EnumResourceProcessingPhase.Queued, queued.Phase, "one run at a time, also of another kind");
        Assert.AreEqual(1, queued.QueuePosition);
        Assert.AreEqual("qwen3.5:9b", queued.Model);
        Assert.IsTrue(jobs.IsIndexBusy(repositoryId));

        runs.Complete(repositoryId);
        await resourcesStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await WaitForAsync(() => !jobs.IsResourcesBusy(repositoryId));
        var done = jobs.GetResourcesProgress(repositoryId)!;
        Assert.AreEqual(EnumResourceProcessingPhase.Completed, done.Phase);
        Assert.AreEqual(2, done.PhotosDescribed);
        Assert.AreEqual("Repo", done.RepositoryName);
        Assert.AreEqual("admin", done.RequestedBy);
        await jobs.StopAsync(CancellationToken.None);
    }

    [TestMethod]
    public async Task CancelResources_AQueuedRun_LeavesTheQueue_AndTheIndexGoesOn()
    {
        var (jobs, runs) = Create();
        await jobs.StartAsync(CancellationToken.None);
        var repositoryId = Guid.NewGuid();
        jobs.EnqueueIndex(repositoryId, "Repo", "admin", false);
        await WaitForAsync(() => runs.HasStarted(repositoryId));
        jobs.EnqueueResources(repositoryId, "Repo", "admin", new AiProviderRef { Model = "m" }, new ResourceProcessingOptionsDto());

        Assert.IsTrue(jobs.CancelResources(repositoryId));
        Assert.AreEqual(EnumResourceProcessingPhase.Canceled, jobs.GetResourcesProgress(repositoryId)!.Phase);
        Assert.IsTrue(jobs.IsIndexBusy(repositoryId));
        Assert.IsFalse(jobs.CancelResources(repositoryId), "nothing left to cancel");

        runs.Complete(repositoryId);
        await WaitForAsync(() => !jobs.IsIndexBusy(repositoryId));
        await jobs.StopAsync(CancellationToken.None);
    }
}
