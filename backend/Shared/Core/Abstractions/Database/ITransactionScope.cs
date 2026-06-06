using CSharpFunctionalExtensions;
using Shared.SharedKernel.Failures;

namespace Core.Abstractions.Database;

public interface ITransactionScope : IDisposable
{
    UnitResult<Error> Commit();

    UnitResult<Error> Rollback();
}