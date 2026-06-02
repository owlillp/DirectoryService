using Core.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartmentLocations;

public record UpdateDepartmentLocationsCommand(Guid DepartmentId, UpdateDepartmentLocationsRequest Request) : ICommand;