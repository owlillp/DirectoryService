using Shared.Failures;

namespace DirectoryService.Application.Positions.Failures;

public static class PositionsErrors
{
    public static Error Inactive(Guid positionId)
        => Error.Validation("position.inactive", $"Position with id [{positionId}] is inactive");
}