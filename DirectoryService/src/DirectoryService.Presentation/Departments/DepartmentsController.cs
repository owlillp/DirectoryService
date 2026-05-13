using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.Commands.CreateDepartment;
using DirectoryService.Application.Departments.Commands.UpdateDepartmentLocations;
using DirectoryService.Application.Departments.Commands.UpdateDepartmentParent;
using DirectoryService.Application.Departments.Queries.GetTopDepartmentsByPositions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Contracts.Departments.Responses;
using Microsoft.AspNetCore.Mvc;
using Shared.EndpointResults;

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

    [HttpPut("{departmentId:guid}/parent")]
    public async Task<EndpointResult> UpdateDepartmentParent(
        [FromServices] ICommandHandler<UpdateDepartmentParentCommand> handler,
        [FromRoute] Guid departmentId,
        [FromBody] UpdateDepartmentParentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDepartmentParentCommand(departmentId, request);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpGet("top-positions")]
    public async Task<EndpointResult<GetTopDepartmentsByPositionsResponse>> GetTopByPositionsCount(
        [FromServices] IQueryHandler<GetTopDepartmentsByPositionsResponse, GetTopDepartmentsByPositionQuery> handler,
        [FromQuery] int topCount,
        CancellationToken cancellationToken)
    {
        var query = new GetTopDepartmentsByPositionQuery(topCount);
        return await handler.Handle(query, cancellationToken);
    }
}