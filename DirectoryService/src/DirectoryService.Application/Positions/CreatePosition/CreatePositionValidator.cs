using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Shared.Failures;

namespace DirectoryService.Application.Positions.CreatePosition;

public class CreatePositionValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionValidator()
    {
        RuleFor(c => c.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(CreatePositionCommand.Request)));

        RuleFor(c => c.Request.Name)
            .MustBeValueObject(PositionName.Create);

        RuleFor(c => c.Request.Description)
            .MustBeNullableValueObject(PositionDescription.Create);

        RuleFor(c => c.Request.DepartmentIds)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(CreatePositionCommand.Request.DepartmentIds)));

        RuleFor(p => p.Request.DepartmentIds)
            .Must(departmentIds => departmentIds.Any())
            .WithError(GeneralErrors.InvalidLength(nameof(CreatePositionCommand.Request.DepartmentIds)));

        RuleFor(p => p.Request.DepartmentIds)
            .Must(departmentIds =>
            {
                var enumerable = departmentIds.ToArray();
                return enumerable.Distinct().Count() == enumerable.Length;
            })
            .WithError(GeneralErrors.Duplicate(nameof(CreatePositionCommand.Request.DepartmentIds)));
    }
}