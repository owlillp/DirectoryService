using CSharpFunctionalExtensions;
using Shared.SharedKernel.Failures;

namespace FileService.Domain;

public sealed record MediaOwner
{
    public static readonly HashSet<string> AllowedContexts =
    [
        "location",
        "position",
        "department",
        "user"
    ];

    public string Context { get; } = null!;
    public Guid EntityId { get; }

    // EF Core
    private MediaOwner() { }

    private MediaOwner(string context, Guid entityId)
    {
        Context = context;
        EntityId = entityId;
    }

    public static Result<MediaOwner, Error> Create(string context, Guid entityId)
    {
        if (string.IsNullOrWhiteSpace(context) || context.Length > 50)
            return GeneralErrors.ValueIsInvalid(nameof(context));

        string normalizedContext = context.Trim().ToLowerInvariant();
        if (!AllowedContexts.Contains(normalizedContext))
            return GeneralErrors.ValueIsInvalid(nameof(context));

        if (entityId == Guid.Empty)
            return GeneralErrors.ValueIsInvalid(nameof(entityId));

        return new MediaOwner(normalizedContext, entityId);
    }

    public static Result<MediaOwner, Error> ForLocation(Guid locationId) => Create("location", locationId);
    public static Result<MediaOwner, Error> ForPosition(Guid positionId) => Create("position", positionId);
    public static Result<MediaOwner, Error> ForDepartment(Guid departmentId) => Create("department", departmentId);
    public static Result<MediaOwner, Error> ForUser(Guid userId) => Create("user", userId);
}