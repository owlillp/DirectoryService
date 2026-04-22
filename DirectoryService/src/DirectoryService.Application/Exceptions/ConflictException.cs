using Shared.Failures;

namespace DirectoryService.Application.Exceptions;

public class ConflictException(Error error) : Exception(error.Message)
{
    public Error Error { get; } = error;
}