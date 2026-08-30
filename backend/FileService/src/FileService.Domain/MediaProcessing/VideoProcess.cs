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

    public int ProcessPercentage { get; private set; }

    public string? ErrorMessage { get; private set; }

    public bool IsCriticalError { get; private set; }

    public int RetryCount { get; private set; }

    public int MaxRetryCount { get; private set; } = 3;

    public DateTime? NextRetryAt { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public IReadOnlyList<ProcessingStep> Steps => _steps.AsReadOnly();

    public ProcessingStep? CurrentStep => _steps.FirstOrDefault(s => s.Status == StepStatus.IN_PROGRESS);

    // Ef Core
    private VideoProcess()
    { }

    private VideoProcess(Guid videoAssetId)
    {
        Id = Guid.NewGuid();
        VideoAssetId = videoAssetId;
        Status = ProcessingStatus.IN_PROGRESS;
        ProcessPercentage = 0;
        StartedAt = DateTime.UtcNow;

        InitializeSteps();
    }

    private void InitializeSteps()
    {
        int order = 0;
        foreach (var (type, weight) in _stepWeights)
        {
            _steps.Add(new ProcessingStep(type, order++, weight));
        }
    }
}

public enum ProcessingStatus
{
    IN_PROGRESS,
    COMPLETED,
    FAILED,
}