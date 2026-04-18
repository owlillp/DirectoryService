using System.Collections;

namespace Shared.Failures;

public class Errors : IEnumerable<Error>
{
    private readonly List<Error> _errors;

    public Errors(IEnumerable<Error> errors)
        => _errors = [..errors];

    public Errors(Error error)
        => _errors = [error];

    public IEnumerator<Error> GetEnumerator()
        => _errors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public static implicit operator Errors(Error[] errors)
        => new(errors);

    public static implicit operator Errors(Error error)
        => new([error]);
}