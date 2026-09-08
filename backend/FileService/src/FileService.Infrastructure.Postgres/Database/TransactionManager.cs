using Core.Abstractions.Database;
using CSharpFunctionalExtensions;
using FileService.Application.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;
using Wolverine.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres.Database;

public class TransactionManager(
    ILoggerFactory loggerFactory,
    IDbContextOutbox<FileServiceDbContext> outbox,
    ILogger<TransactionManager> logger)
    : ITransactionManager
{
    private IDbContextTransaction? _currentTransaction;

    public async Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        try
        {
            _currentTransaction = await outbox.DbContext.Database.BeginTransactionAsync(cancellationToken);

            var transactionScopeLogger = loggerFactory.CreateLogger<TransactionScope>();
            var transactionScope = new TransactionScope(_currentTransaction.GetDbTransaction(), transactionScopeLogger);

            return transactionScope;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to begin transaction");
            return Error.Failure("transaction.begin.failed", "Failed to begin transaction");
        }
    }

    public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await outbox.DbContext.SaveChangesAsync(cancellationToken);
            }
            else
            {
                await outbox.SaveChangesAndFlushMessagesAsync(cancellationToken);
            }

            return UnitResult.Success<Error>();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict while saving changes.");
            return GeneralErrors.Failure("concurrency.conflict");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation canceled while saving changes.");
            return GeneralErrors.Failure("save.changes");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving changes to database.");
            return GeneralErrors.Failure("save.changes");
        }
    }

    public async Task<UnitResult<Error>> CommitTransactionAsync(CancellationToken cancellationToken)
    {
        if (_currentTransaction == null)
        {
            return GeneralErrors.Failure("Transaction not found");
        }

        try
        {
            await outbox.DbContext.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
            await outbox.FlushOutgoingMessagesAsync();
            return UnitResult.Success<Error>();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict while saving changes.");
            await RollbackAsync(cancellationToken);
            return GeneralErrors.Failure("concurrency conflict");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation canceled while saving changes.");
            await RollbackAsync(cancellationToken);
            return GeneralErrors.Failure("save.changes");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error committing transaction.");
            await RollbackAsync(cancellationToken);
            return GeneralErrors.Failure("transaction.rollback");
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    private async Task RollbackAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to rollback transaction.");
        }
    }
}