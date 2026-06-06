using Shared.SharedKernel.Failures;

namespace Shared.SharedKernel.Exceptions;

public class ConflictException(Error error) : Exception(error.Message)
{
    public Error Error { get; } = error;
}