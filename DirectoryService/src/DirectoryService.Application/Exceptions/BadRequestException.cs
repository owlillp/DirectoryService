using Shared.Failures;

namespace DirectoryService.Application.Exceptions;

public class BadRequestException(Error error): Exception(error.Message)
{
    public Error Error { get; } = error;
}