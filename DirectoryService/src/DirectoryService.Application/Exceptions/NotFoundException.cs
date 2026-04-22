using Shared.Failures;

namespace DirectoryService.Application.Exceptions;

public class NotFoundException(Error error) : Exception(error.Message)
{
    public Error Error { get; } = error;
}