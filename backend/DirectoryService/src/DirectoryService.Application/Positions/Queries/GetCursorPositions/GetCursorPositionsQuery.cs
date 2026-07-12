using Core.Abstractions;
using DirectoryService.Contracts.Positions.Requests;

namespace DirectoryService.Application.Positions.Queries.GetCursorPositions;

public record GetCursorPositionsQuery(GetCursorPositionsRequest Request) : IQuery;