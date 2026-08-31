using FileService.Domain.MediaProcessing;

namespace FileService.UnitTests.MediaProcessing;

// ProcessingStep internal methods (Start/Complete/Fail/Reset) are not directly
// callable from this test assembly (no InternalsVisibleTo). These tests verify
// ProcessingStep state transitions through the public VideoProcess API, which is
// the only way the step lifecycle is driven in production.
public class ProcessingStepTests
{
    [Fact]
    public void Step_InitialState_IsPending()
    {
        var process = new VideoProcess(Guid.NewGuid());

        var step = process.Steps.First();

        Assert.Equal(StepStatus.PENDING, step.Status);
        Assert.Null(step.StartedAt);
        Assert.Null(step.CompletedAt);
        Assert.Null(step.ResultData);
        Assert.Null(step.ErrorMessage);
        Assert.Equal(StepStatus.PENDING, step.Status);
    }

    [Fact]
    public void Step_WhenStarted_TransitionsToInProgressAndSetsStartedAt()
    {
        var process = new VideoProcess(Guid.NewGuid());
        var step = process.Steps.First();

        process.ProcessNextStep();

        Assert.Equal(StepStatus.IN_PROGRESS, step.Status);
        Assert.NotNull(step.StartedAt);
        Assert.Null(step.CompletedAt);
        Assert.Null(step.ResultData);
        Assert.Null(step.ErrorMessage);
    }

    [Fact]
    public void Step_WhenCompleted_TransitionsToCompletedAndSetsTimestamps()
    {
        var process = new VideoProcess(Guid.NewGuid());
        var step = process.Steps.First();

        process.ProcessNextStep();
        var start = step.StartedAt;
        process.CompleteCurrentStep();

        Assert.Equal(StepStatus.COMPLETED, step.Status);
        Assert.Equal(start, step.StartedAt);
        Assert.NotNull(step.CompletedAt);
        Assert.Null(step.ErrorMessage);
    }

    [Fact]
    public void Step_WhenFailed_TransitionsToFailedAndSetsErrorMessageAndCompletedAt()
    {
        var process = new VideoProcess(Guid.NewGuid());
        var step = process.Steps.First();

        process.ProcessNextStep();
        process.FailCurrentStep("failed step");

        Assert.Equal(StepStatus.FAILED, step.Status);
        Assert.Equal("failed step", step.ErrorMessage);
        Assert.NotNull(step.CompletedAt);
        Assert.Null(step.ResultData);
    }

    [Fact]
    public void Step_AfterReset_ReturnsToPendingAndClearsState()
    {
        var process = new VideoProcess(Guid.NewGuid());
        var step = process.Steps.First();

        process.ProcessNextStep();
        process.CompleteCurrentStep("result-data");
        process.Fail("overall failure");
        process.Reset();

        Assert.Equal(StepStatus.PENDING, step.Status);
        Assert.Null(step.StartedAt);
        Assert.Null(step.CompletedAt);
        Assert.Null(step.ResultData);
        Assert.Null(step.ErrorMessage);
    }

    [Fact]
    public void Step_EachStepHasUniqueId()
    {
        var process = new VideoProcess(Guid.NewGuid());

        var ids = process.Steps.Select(s => s.Id).Distinct().ToArray();

        Assert.Equal(process.Steps.Count, ids.Length);
    }
}
