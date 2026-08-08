using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations.Dtos;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Locations.Queries.GetLocation;

public class GetLocationHandler(
    IValidator<GetLocationQuery> validator,
    IReadDbContext dbContext)
    : IQueryHandler<LocationDto, GetLocationQuery>
{
    public async Task<Result<LocationDto, Errors>> Handle(GetLocationQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var locationId = new LocationId(query.LocationId);

        var location = await dbContext
            .LocationsRead
            .Include(l => l.Departments)
            .FirstOrDefaultAsync(l => l.Id == locationId, cancellationToken);

        if (location == null)
        {
            return GeneralErrors.NotFound(nameof(Location), locationId.Value).ToErrors();
        }

        return new LocationDto
        {
            Id = location.Id.Value,
            Name = location.Name.Value,
            TimeZone = location.Timezone.Value,
            IsActive = location.IsActive,
            CreatedAt = location.CreatedAt,
            DepartmentIds = location.Departments.Select(d => d.DepartmentId.Value).ToList(),
            Address = new LocationAddressDto
            {
                Country = location.Address.Country,
                City = location.Address.City,
                Street = location.Address.Street,
                Apartment = location.Address.Apartment,
                PostalCode = location.Address.PostalCode,
                BuildingNumber = location.Address.BuildingNumber,
            },
            PreviewId = location.PreviewId,
        };
    }
}