using Shared.SharedKernel.Failures;

namespace Shared.SharedKernel.Exceptions;

public class BadRequestException(Error error): Exception(error.Message)
{
    public Error Error { get; } = error;
}