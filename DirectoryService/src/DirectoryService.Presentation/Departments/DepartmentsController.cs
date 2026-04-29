using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.UpdateDepartmentLocations;
using DirectoryService.Contracts.Departments;
using DirectoryService.Presentation.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Departments;

[ApiController]
[Route("/api/[controller]")]
public class DepartmentsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreateDepartmentCommand> handler,
        [FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateDepartmentCommand(request);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpPatch("{departmentId:guid}/locations")]
    public async Task<EndpointResult<Guid>> UpdateDepartmentLocations(
        [FromServices] ICommandHandler<Guid, UpdateDepartmentLocationsCommand> handler,
        [FromRoute] Guid departmentId,
        [FromBody] UpdateDepartmentLocationsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDepartmentLocationsCommand(departmentId, request);
        return await handler.Handle(command, cancellationToken);
    }
}