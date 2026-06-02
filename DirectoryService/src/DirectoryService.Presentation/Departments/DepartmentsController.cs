using Core.Abstractions;
using DirectoryService.Application.Departments.Commands.CreateDepartment;
using DirectoryService.Application.Departments.Commands.LinkLocation;
using DirectoryService.Application.Departments.Commands.LinkPosition;
using DirectoryService.Application.Departments.Commands.SoftDeleteDepartment;
using DirectoryService.Application.Departments.Commands.UnlinkLocation;
using DirectoryService.Application.Departments.Commands.UnlinkPosition;
using DirectoryService.Application.Departments.Commands.UpdateDepartment;
using DirectoryService.Application.Departments.Commands.UpdateDepartmentLocations;
using DirectoryService.Application.Departments.Commands.UpdateDepartmentParent;
using DirectoryService.Application.Departments.Queries.GetChildDepartments;
using DirectoryService.Application.Departments.Queries.GetDepartment;
using DirectoryService.Application.Departments.Queries.GetDepartmentAncestors;
using DirectoryService.Application.Departments.Queries.GetRootDepartments;
using DirectoryService.Application.Departments.Queries.GetTopDepartmentsByPositions;
using DirectoryService.Application.Departments.Queries.SearchDepartments;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Departments.Dtos;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Contracts.Departments.Responses;
using Framework.EndpointResults;
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

    [HttpPost("{departmentId:guid}/positions/{positionId:guid}")]
    public async Task<EndpointResult> LinkPosition(
        [FromServices] ICommandHandler<LinkPositionCommand> handler,
        [FromRoute] Guid departmentId,
        [FromRoute] Guid positionId,
        CancellationToken cancellationToken)
    {
        var command = new LinkPositionCommand(departmentId, positionId);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpPost("{departmentId:guid}/locations/{locationId:guid}")]
    public async Task<EndpointResult> LinkLocation(
        [FromServices] ICommandHandler<LinkLocationCommand> handler,
        [FromRoute] Guid departmentId,
        [FromRoute] Guid locationId,
        CancellationToken cancellationToken)
    {
        var command = new LinkLocationCommand(departmentId, locationId);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpPatch("{departmentId:guid}")]
    public async Task<EndpointResult> Update(
        [FromServices] ICommandHandler<UpdateDepartmentCommand> handler,
        [FromRoute] Guid departmentId,
        [FromBody] UpdateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDepartmentCommand(departmentId, request);
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

    [HttpDelete("{departmentId:guid}/positions/{positionId:guid}")]
    public async Task<EndpointResult> UnlinkPosition(
        [FromServices] ICommandHandler<UnlinkPositionCommand> handler,
        [FromRoute] Guid departmentId,
        [FromRoute] Guid positionId,
        CancellationToken cancellationToken)
    {
        var command = new UnlinkPositionCommand(departmentId, positionId);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpDelete("{departmentId:guid}/locations/{locationId:guid}")]
    public async Task<EndpointResult> UnlinkLocation(
        [FromServices] ICommandHandler<UnlinkLocationCommand> handler,
        [FromRoute] Guid departmentId,
        [FromRoute] Guid locationId,
        CancellationToken cancellationToken)
    {
        var command = new UnlinkLocationCommand(departmentId, locationId);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpDelete("{departmentId:guid}")]
    public async Task<EndpointResult> SoftDelete(
        [FromRoute] Guid departmentId,
        [FromServices] ICommandHandler<SoftDeleteDepartmentCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new SoftDeleteDepartmentCommand(departmentId);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpGet("{departmentId:guid}")]
    public async Task<EndpointResult<DepartmentDto>> Get(
        [FromServices] IQueryHandler<DepartmentDto, GetDepartmentQuery> handler,
        [FromRoute] Guid departmentId,
        CancellationToken cancellationToken)
    {
        var query = new GetDepartmentQuery(departmentId);
        return await handler.Handle(query, cancellationToken);
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

    [HttpGet("roots")]
    public async Task<EndpointResult<PagedResult<DepartmentWithChildrenDto>>> GetRootDepartments(
        [FromServices] IQueryHandler<PagedResult<DepartmentWithChildrenDto>, GetRootDepartmentsQuery> handler,
        [FromQuery] GetRootDepartmentsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetRootDepartmentsQuery(request);
        return await handler.Handle(query, cancellationToken);
    }

    [HttpGet("{parentId:guid}/children")]
    public async Task<EndpointResult<GetChildDepartmentsResponse>> GetChildDepartmentsByParentId(
        [FromServices] IQueryHandler<GetChildDepartmentsResponse, GetChildDepartmentsQuery> handler,
        [FromRoute] Guid parentId,
        [FromQuery] PaginationRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetChildDepartmentsQuery(parentId, request);
        return await handler.Handle(query, cancellationToken);
    }

    [HttpGet("{departmentId:guid}/ancestors")]
    public async Task<EndpointResult<GetDepartmentAncestorsResponse>> GetAncestors(
        [FromServices] IQueryHandler<GetDepartmentAncestorsResponse, GetDepartmentAncestorsQuery> handler,
        [FromRoute] Guid departmentId,
        CancellationToken cancellationToken)
    {
        var query = new GetDepartmentAncestorsQuery(departmentId);
        return await handler.Handle(query, cancellationToken);
    }

    [HttpGet("tree")]
    public async Task<EndpointResult<PagedResult<AncestorDepartmentDto>>> Search(
        [FromServices] IQueryHandler<PagedResult<AncestorDepartmentDto>, SearchDepartmentsQuery> handler,
        [FromQuery] SearchDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var query = new SearchDepartmentsQuery(request);
        return await handler.Handle(query, cancellationToken);
    }
}