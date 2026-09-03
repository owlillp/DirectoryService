using CSharpFunctionalExtensions;
using Shared.SharedKernel.Failures;

namespace FileService.Domain.MediaProcessing;

public class VideoProcess
{
    private static readonly Dictionary<StepType, int> _stepWeights = new()
    {
        { StepType.INITIALIZE, 0 },
        { StepType.EXTRACT_METADATA, 10 },
        { StepType.GENERATE_HLS, 60 },
        { StepType.UPLOAD_HLS, 15 },
        { StepType.GENERATE_PREVIEW, 10 },
        { StepType.CLEANUP, 5 },
    };

    private readonly List<ProcessingStep> _steps = [];

    public Guid Id { get; private set; }

    public Guid VideoAssetId { get; private set; }

    public ProcessingStatus Status { get; private set; }

    public int ProgressPercentage { get; private set; }

    public string? ErrorMessage { get; private set; }

    public bool IsCriticalError { get; private set; }

    public int RetryCount { get; private set; }

    public int MaxRetryCount { get; private set; } = 3;

    public DateTime? NextRetryAt { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public IReadOnlyList<ProcessingStep> Steps => _steps.AsReadOnly();

    public ProcessingStep? CurrentStep => _steps.FirstOrDefault(s => s.Status == StepStatus.IN_PROGRESS);

    public VideoProcess(Guid videoAssetId)
    {
        Id = Guid.NewGuid();
        VideoAssetId = videoAssetId;
        Status = ProcessingStatus.IN_PROGRESS;
        ProgressPercentage = 0;
        StartedAt = DateTime.UtcNow;

        InitializeSteps();
    }

    // Ef Core
    private VideoProcess()
    { }

    public Result<ProcessingStep?, Error> ProcessNextStep()
    {
        if (Status != ProcessingStatus.IN_PROGRESS)
        {
            return Error.Validation(
                "processing.invalid.status",
                $"Cannot process step with status: {Status}");
        }

        ProcessingStep? currentStep = CurrentStep;
        if (currentStep != null)
        {
            return currentStep;
        }

        ProcessingStep? nextStep = _steps
            .OrderBy(s => s.Order)
            .FirstOrDefault(s => s.Status == StepStatus.PENDING);

        if (nextStep == null)
        {
            Complete();
            return Result.Success<ProcessingStep?, Error>(null);
        }

        var startResult = nextStep.Start();
        if (startResult.IsFailure)
        {
            return startResult.Error;
        }

        return nextStep;
    }

    public UnitResult<Error> CompleteCurrentStep(string? resultData = null)
    {
        if (Status != ProcessingStatus.IN_PROGRESS)
        {
            return Error.Validation(
                "processing.invalid.status",
                $"Cannot complete current step with status: {Status}");
        }

        ProcessingStep? currentStep = CurrentStep;
        if (currentStep == null)
        {
            return Error.Validation("processing.no.active.step", "No active step to complete");
        }

        var completeResult = currentStep.Complete();
        if (completeResult.IsFailure)
        {
            return completeResult.Error;
        }

        RecalculateProgress();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> FailCurrentStep(string errorMessage)
    {
        if (Status != ProcessingStatus.IN_PROGRESS)
        {
            return Error.Validation(
                "processing.invalid.status",
                $"Cannot fail current step with status: {Status}");
        }

        ProcessingStep? currentStep = CurrentStep;
        if (currentStep == null)
        {
            return Error.Validation("processing.no.active.step", "No active step to fail");
        }

        var failResult = currentStep.Fail(errorMessage);
        if (failResult.IsFailure)
        {
            return failResult.Error;
        }

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Fail(string errorMessage, bool isCriticalError = false)
    {
        if (Status != ProcessingStatus.IN_PROGRESS)
        {
            return Error.Validation(
                "processing.invalid.status",
                $"Cannot fail process with status: {Status}");
        }

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            return Error.Validation("processing.error.required", "Error message is required");
        }

        Status = ProcessingStatus.FAILED;
        ErrorMessage = errorMessage;
        IsCriticalError = isCriticalError;
        CompletedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public bool CanRetry() => RetryCount < MaxRetryCount && !IsCriticalError;

    public UnitResult<Error> Reset()
    {
        if (Status != ProcessingStatus.FAILED)
        {
            return Error.Validation(
                "processing.invalid.status",
                $"Cannot retry process with status: {Status}");
        }

        Status = ProcessingStatus.IN_PROGRESS;
        ProgressPercentage = 0;
        CompletedAt = null;
        ErrorMessage = null;
        IsCriticalError = false;

        foreach (var step in _steps)
        {
            step.Reset();
        }

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> ScheduleRetry(DateTime nextRetryAt)
    {
        if (Status != ProcessingStatus.FAILED)
        {
            return Error.Validation(
                "processing.invalid.status",
                $"Cannot schedule retry process with status: {Status}");
        }

        if (IsCriticalError)
        {
            return Error.Validation(
                "processing.retry.critical",
                "Cannot retry critical failure");
        }

        if (RetryCount >= MaxRetryCount)
        {
            return Error.Validation(
                "processing.retry.exhausted",
                "Max retry exceeded");
        }

        RetryCount++;
        NextRetryAt = nextRetryAt;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Complete()
    {
        if (Status != ProcessingStatus.IN_PROGRESS)
        {
            return Error.Validation(
                "processing.invalid.status",
                $"Cannot complete step with status: {Status}");
        }

        bool allStepsCompleted = _steps.All(s => s.Status == StepStatus.COMPLETED);
        if (!allStepsCompleted)
        {
            return Error.Validation("processing.incomplete.steps", "Cannot complete steps while any steps in progress");
        }

        Status = ProcessingStatus.COMPLETED;
        CompletedAt = DateTime.UtcNow;
        ProgressPercentage = 100;

        return UnitResult.Success<Error>();
    }

    private void InitializeSteps()
    {
        int order = 0;
        foreach (var (type, weight) in _stepWeights)
        {
            _steps.Add(new ProcessingStep(type, order++, weight));
        }
    }

    private void RecalculateProgress()
    {
        int totalProgress = _steps
            .Where(s => s.Status == StepStatus.COMPLETED)
            .Sum(s => s.Weight);

        ProgressPercentage = totalProgress;
    }
}

public enum ProcessingStatus
{
    IN_PROGRESS,
    COMPLETED,
    FAILED,
}