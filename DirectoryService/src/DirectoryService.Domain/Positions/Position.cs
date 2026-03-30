using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Positions;

public class Position
{
    private Position() { }

    private List<DepartmentPosition> _departments = [];

    private Position(Guid id, PositionName name, PositionDescription? description)
    {
        Id = id;
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private init; }

    public PositionName Name { get; private set; } = null!;

    public PositionDescription? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentPosition> Departments => _departments;

    public static Result<Position, Error> Create(
        string name,
        string? description,
        Guid? id = null)
    {
        var nameResult = PositionName.Create(name);
        if (nameResult.IsFailure)
        {
            return nameResult.Error;
        }

        PositionDescription? positionDescription = null;
        if (!string.IsNullOrWhiteSpace(description))
        {
            var descriptionResult = PositionDescription.Create(description);
            if (descriptionResult.IsFailure)
            {
                return descriptionResult.Error;
            }

            positionDescription = descriptionResult.Value;
        }

        return new Position(
            id ?? Guid.NewGuid(),
            nameResult.Value,
            positionDescription);
    }
}