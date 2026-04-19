using CSharpFunctionalExtensions;
using Shared.Failures;

namespace DirectoryService.Application.Abstractions;

public interface ICommand;

public interface ICommandHandler<TResponse, in TCommand>
     where TCommand : ICommand
{
     Task<Result<TResponse, Errors>> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task<Result<Errors>> Handle(TCommand command, CancellationToken cancellationToken);
}