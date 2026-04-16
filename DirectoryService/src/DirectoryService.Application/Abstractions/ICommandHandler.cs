using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared.Errors;

namespace DirectoryService.Application.Abstractions;

public interface ICommand;

public interface ICommandHandler<TResponse, in TCommand>
     where TCommand : ICommand
{
     Task<Result<TResponse, Error>> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task<Result<Error>> Handle(TCommand command, CancellationToken cancellationToken);
}