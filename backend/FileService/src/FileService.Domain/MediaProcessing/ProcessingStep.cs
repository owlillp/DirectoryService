namespace FileService.Domain.MediaProcessing;

public sealed class ProcessingStep
{
    public Guid Id { get; private set; }

    public StepType StepType { get; private set; }

    public int Order { get; private set; }

    public int Weight { get; private set; }

    public StepStatus Status { get; private set; }

    public string? ResultData { get; private set; }

    public string? ErrorMessage { get; private set; }

    public DateTime? StartedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public ProcessingStep(StepType stepType, int order, int weight)
    {
        Id = Guid.NewGuid();
        StepType = stepType;
        Order = order;
        Weight = weight;
        Status = StepStatus.PENDING;
    }

    // Ef Core
    private ProcessingStep()
    { }
}

public enum StepType
{
    INITIALIZE,
    EXTRACT_METADATA,
    GENERATE_HLS,
    UPLOAD_HLS,
    GENERATE_PREVIEW,
    CLEANUP,
}

public enum StepStatus
{
    PENDING,
    IN_PROGRESS,
    COMPLETED,
    FAILED,
}