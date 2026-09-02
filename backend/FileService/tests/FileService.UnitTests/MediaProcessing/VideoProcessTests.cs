using FileService.Domain.MediaProcessing;

namespace FileService.UnitTests.MediaProcessing;

public class VideoProcessTests
{
    private static readonly Guid ValidVideoAssetId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithVideoAssetId_InitializesInProgress()
    {
        var process = new VideoProcess(ValidVideoAssetId);

        Assert.Equal(ValidVideoAssetId, process.VideoAssetId);
        Assert.NotEqual(Guid.Empty, process.Id);
        Assert.Equal(ProcessingStatus.IN_PROGRESS, process.Status);
        Assert.Equal(0, process.ProgressPercentage);
        Assert.Equal(3, process.MaxRetryCount);
        Assert.Equal(0, process.RetryCount);
        Assert.False(process.IsCriticalError);
    }

    [Fact]
    public void Constructor_InitializesSixStepsInWorkflowOrder()
    {
        var process = new VideoProcess(ValidVideoAssetId);

        Assert.Equal(6, process.Steps.Count);
        Assert.Equal(
            [
                StepType.INITIALIZE,
                StepType.EXTRACT_METADATA,
                StepType.GENERATE_HLS,
                StepType.UPLOAD_HLS,
                StepType.GENERATE_PREVIEW,
                StepType.CLEANUP
            ],
            process.Steps.Select(s => s.StepType));
    }

    [Fact]
    public void Constructor_InitializesAllStepsAsPending()
    {
        var process = new VideoProcess(ValidVideoAssetId);

        Assert.All(process.Steps, step => Assert.Equal(StepStatus.PENDING, step.Status));
        Assert.Null(process.CurrentStep);
    }

    [Fact]
    public void Constructor_AssignsCorrectWeightsToSteps()
    {
        var process = new VideoProcess(ValidVideoAssetId);

        var stepsByType = process.Steps.ToDictionary(s => s.StepType);

        Assert.Equal(0, stepsByType[StepType.INITIALIZE].Weight);
        Assert.Equal(10, stepsByType[StepType.EXTRACT_METADATA].Weight);
        Assert.Equal(60, stepsByType[StepType.GENERATE_HLS].Weight);
        Assert.Equal(15, stepsByType[StepType.UPLOAD_HLS].Weight);
        Assert.Equal(10, stepsByType[StepType.GENERATE_PREVIEW].Weight);
        Assert.Equal(5, stepsByType[StepType.CLEANUP].Weight);
    }

    [Fact]
    public void Constructor_AssignsSequentialOrderToSteps()
    {
        var process = new VideoProcess(ValidVideoAssetId);

        var orders = process.Steps.Select(s => s.Order).ToArray();
        Assert.Equal(new[] { 0, 1, 2, 3, 4, 5 }, orders);
    }

    [Fact]
    public void ProcessNextStep_WhenNoActiveStep_StartsFirstPendingStep()
    {
        var process = new VideoProcess(ValidVideoAssetId);

        var result = process.ProcessNextStep();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(StepType.INITIALIZE, result.Value!.StepType);
        Assert.Equal(StepStatus.IN_PROGRESS, result.Value.Status);
        Assert.Equal(process.CurrentStep, result.Value);
    }

    [Fact]
    public void ProcessNextStep_WhenActiveStepExists_ReturnsCurrentActiveStep()
    {
        var process = new VideoProcess(ValidVideoAssetId);
        process.ProcessNextStep();

        var result = process.ProcessNextStep();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(StepType.INITIALIZE, result.Value!.StepType);
        Assert.Equal(StepStatus.IN_PROGRESS, result.Value.Status);
    }

    [Fact]
    public void ProcessNextStep_MovesThroughStepsInOrder()
    {
        var process = new VideoProcess(ValidVideoAssetId);

        var stepTypes = new List<StepType?>();
        while (true)
        {
            var result = process.ProcessNextStep();
            if (result.IsFailure || result.Value == null)
            {
                break;
            }

            stepTypes.Add(result.Value.StepType);
            process.CompleteCurrentStep();
        }

        Assert.Equal(
            new StepType?[]
            {
                StepType.INITIALIZE,
                StepType.EXTRACT_METADATA,
                StepType.GENERATE_HLS,
                StepType.UPLOAD_HLS,
                StepType.GENERATE_PREVIEW,
                StepType.CLEANUP,
            },
            stepTypes);
    }

    [Fact]
    public void ProcessNextStep_WhenAllStepsCompleted_CompletesProcess()
    {
        var process = new VideoProcess(ValidVideoAssetId);
        RunFullWorkflow(process);

        var result = process.ProcessNextStep();

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(ProcessingStatus.COMPLETED, process.Status);
        Assert.Equal(100, process.ProgressPercentage);
        Assert.NotNull(process.CompletedAt);
    }

    [Fact]
    public void ProcessNextStep_AfterProcessCompleted_ReturnsFailure()
    {
        var process = new VideoProcess(ValidVideoAssetId);
        RunFullWorkflow(process);
        var completion = process.ProcessNextStep();
        Assert.True(completion.IsSuccess);
        Assert.Equal(ProcessingStatus.COMPLETED, process.Status);

        var result = process.ProcessNextStep();

        Assert.True(result.IsFailure);
        Assert.Equal("processing.invalid.status", result.Error.Code);
    }

    [Fact]
    public void CompleteCurrentStep_WhenNoActiveStep_ReturnsFailure()
    {
        var process = new VideoProcess(ValidVideoAssetId);

        var result = process.CompleteCurrentStep();

        Assert.True(result.IsFailure);
        Assert.Equal("processing.no.active.step", result.Error.Code);
    }

    [Fact]
    public void CompleteCurrentStep_CompletesStepAndSetsTimestamps()
    {
        var process = new VideoProcess(ValidVideoAssetId);
        process.ProcessNextStep();

        var result = process.CompleteCurrentStep("some-result");

        Assert.True(result.IsSuccess);
        var step = process.Steps.Single(s => s.StepType == StepType.INITIALIZE);
        Assert.Equal(StepStatus.COMPLETED, step.Status);
        Assert.NotNull(step.CompletedAt);
    }

    [Fact]
    public void CompleteCurrentStep_WhenProcessCompleted_ReturnsFailure()
    {
        var process = new VideoProcess(ValidVideoAssetId);
        RunFullWorkflow(process);
        process.ProcessNextStep();
        Assert.Equal(ProcessingStatus.COMPLETED, process.Status);

        var result = process.CompleteCurrentStep();

        Assert.True(result.IsFailure);
        Assert.Equal("processing.invalid.status", result.Error.Code);
    }

    [Theory]
    [InlineData(StepType.INITIALIZE, 0)]
    [InlineData(StepType.EXTRACT_METADATA, 10)]
    [InlineData(StepType.GENERATE_HLS, 70)]
    [InlineData(StepType.UPLOAD_HLS, 85)]
    [InlineData(StepType.GENERATE_PREVIEW, 95)]
    [InlineData(StepType.CLEANUP, 100)]
    public void CompleteCurrentStep_RecalculatesProgress(
        StepType stepToFinish,
        int expectedPercentage)
    {
        var process = new VideoProcess(ValidVideoAssetId);

        foreach (var step in process.Steps.OrderBy(s => s.Order))
        {
            process.ProcessNextStep();
            process.CompleteCurrentStep();

            if (step.StepType == stepToFinish)
            {
                break;
            }
        }

        Assert.Equal(expectedPercentage, process.ProgressPercentage);
    }

    [Fact]
    public void FailCurrentStep_WhenNoActiveStep_ReturnsFailure()
    {
        var process = new VideoProcess(ValidVideoAssetId);

        var result = process.FailCurrentStep("boom");

        Assert.True(result.IsFailure);
        Assert.Equal("processing.no.active.step", result.Error.Code);
    }

    [Fact]
    public void FailCurrentStep_FailsActiveStepAndSetsErrorMessage()
    {
        var process = new VideoProcess(ValidVideoAssetId);
        process.ProcessNextStep();

        var result = process.FailCurrentStep("metadata extraction failed");

        Assert.True(result.IsSuccess);
        var step = process.Steps.Single(s => s.StepType == StepType.INITIALIZE);
        Assert.Equal(StepStatus.FAILED, step.Status);
        Assert.Equal("metadata extraction failed", step.ErrorMessage);
        Assert.NotNull(step.CompletedAt);
    }

    [Fact]
    public void Fail_WithValidMessage_SetsProcessFailed()
    {
        var process = new VideoProcess(ValidVideoAssetId);

        var result = process.Fail("unexpected error");

        Assert.True(result.IsSuccess);
        Assert.Equal(ProcessingStatus.FAILED, process.Status);
        Assert.Equal("unexpected error", process.ErrorMessage);
        Assert.False(process.IsCriticalError);
        Assert.NotNull(process.CompletedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Fail_WithNullOrWhiteSpaceMessage_ReturnsFailure(string? message)
    {
        var process = new VideoProcess(ValidVideoAssetId);

        var result = process.Fail(message!);

        Assert.True(result.IsFailure);
        Assert.Equal("processing.error.required", result.Error.Code);
    }

    [Fact]
    public void Fail_WhenProcessCompleted_ReturnsFailure()
    {
        var process = new VideoProcess(ValidVideoAssetId);
        RunFullWorkflow(process);
        process.ProcessNextStep();
        Assert.Equal(ProcessingStatus.COMPLETED, process.Status);

        var result = process.Fail("too late");

        Assert.True(result.IsFailure);
        Assert.Equal("processing.invalid.status", result.Error.Code);
    }

    [Fact]
    public void Fail_WithIsCriticalError_SetsCriticalFlag()
    {
        var process = new VideoProcess(ValidVideoAssetId);

        var result = process.Fail("critical failure", isCriticalError: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(ProcessingStatus.FAILED, process.Status);
        Assert.True(process.IsCriticalError);
    }

    [Fact]
    public void CanRetry_ForFreshProcess_ReturnsTrue()
    {
        var process = new VideoProcess(ValidVideoAssetId);

        Assert.True(process.CanRetry());
    }

    [Fact]
    public void CanRetry_WhenCriticalError_ReturnsFalse()
    {
        var process = new VideoProcess(ValidVideoAssetId);
        process.Fail("critical failure", isCriticalError: true);

        Assert.False(process.CanRetry());
    }

    [Fact]
    public void CanRetry_WhenMaxRetryReached_ReturnsFalse()
    {
        var process = new VideoProcess(ValidVideoAssetId);
        process.Fail("failure");
        process.ScheduleRetry(DateTime.UtcNow.AddSeconds(30));
        process.Reset();
        process.Fail("failure");
        process.ScheduleRetry(DateTime.UtcNow.AddSeconds(30));
        process.Reset();
        process.Fail("failure");
        process.ScheduleRetry(DateTime.UtcNow.AddSeconds(30));

        Assert.False(process.CanRetry());
    }

    [Fact]
    public void Reset_WhenNotFailed_ReturnsFailure()
    {
        var process = new VideoProcess(ValidVideoAssetId);

        var result = process.Reset();

        Assert.True(result.IsFailure);
        Assert.Equal("processing.invalid.status", result.Error.Code);
    }

    [Fact]
    public void Reset_WhenFailed_ResetsProcessAndSteps()
    {
        var process = new VideoProcess(ValidVideoAssetId);
        process.ProcessNextStep();
        process.CompleteCurrentStep();
        process.Fail("failure");

        var result = process.Reset();

        Assert.True(result.IsSuccess);
        Assert.Equal(ProcessingStatus.IN_PROGRESS, process.Status);
        Assert.Equal(0, process.ProgressPercentage);
        Assert.Null(process.ErrorMessage);
        Assert.Null(process.CompletedAt);
        Assert.False(process.IsCriticalError);
        Assert.All(process.Steps, step =>
        {
            Assert.Equal(StepStatus.PENDING, step.Status);
            Assert.Null(step.ResultData);
            Assert.Null(step.ErrorMessage);
            Assert.Null(step.StartedAt);
            Assert.Null(step.CompletedAt);
        });
    }

    [Fact]
    public void ScheduleRetry_WhenNotFailed_ReturnsFailure()
    {
        var process = new VideoProcess(ValidVideoAssetId);

        var result = process.ScheduleRetry(DateTime.UtcNow.AddSeconds(30));

        Assert.True(result.IsFailure);
        Assert.Equal("processing.invalid.status", result.Error.Code);
    }

    [Fact]
    public void ScheduleRetry_WhenCriticalError_ReturnsFailure()
    {
        var process = new VideoProcess(ValidVideoAssetId);
        process.Fail("critical failure", isCriticalError: true);

        var result = process.ScheduleRetry(DateTime.UtcNow.AddSeconds(30));

        Assert.True(result.IsFailure);
        Assert.Equal("processing.retry.critical", result.Error.Code);
    }

    [Fact]
    public void ScheduleRetry_WhenMaxRetryExceeded_ReturnsFailure()
    {
        var process = new VideoProcess(ValidVideoAssetId);
        process.Fail("failure");
        process.ScheduleRetry(DateTime.UtcNow.AddSeconds(30));
        process.Reset();
        process.Fail("failure");
        process.ScheduleRetry(DateTime.UtcNow.AddSeconds(30));
        process.Reset();
        process.Fail("failure");
        process.ScheduleRetry(DateTime.UtcNow.AddSeconds(30));
        process.Reset();
        process.Fail("failure");

        var result = process.ScheduleRetry(DateTime.UtcNow.AddSeconds(30));

        Assert.True(result.IsFailure);
        Assert.Equal("processing.retry.exhausted", result.Error.Code);
    }

    [Fact]
    public void ScheduleRetry_WhenValid_IncrementsRetryCountAndSetsNextRetryAt()
    {
        var process = new VideoProcess(ValidVideoAssetId);
        process.Fail("failure");
        var nextRetryAt = DateTime.UtcNow.AddMinutes(5);

        var result = process.ScheduleRetry(nextRetryAt);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, process.RetryCount);
        Assert.Equal(nextRetryAt, process.NextRetryAt);
    }

    [Fact]
    public void ScheduleRetry_KeepsProcessFailedUntilReset()
    {
        var process = new VideoProcess(ValidVideoAssetId);
        process.Fail("failure");
        process.ScheduleRetry(DateTime.UtcNow.AddMinutes(5));

        Assert.Equal(ProcessingStatus.FAILED, process.Status);
        Assert.True(process.CanRetry());
    }

    private static void RunFullWorkflow(VideoProcess process)
    {
        var stepCount = process.Steps.Count;
        for (var index = 0; index < stepCount; index++)
        {
            var result = process.ProcessNextStep();
            if (result.IsFailure || result.Value == null)
            {
                break;
            }

            process.CompleteCurrentStep();
        }
    }
}
