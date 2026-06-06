using Shared.SharedKernel.Failures;

namespace Shared.SharedKernel.Exceptions;

public class FailureException(Error error) : Exception(error.Message)
{
    public Error Error { get; } = error;
}