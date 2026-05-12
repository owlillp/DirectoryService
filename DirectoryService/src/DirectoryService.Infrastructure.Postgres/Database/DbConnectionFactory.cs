using System.Data;
using DirectoryService.Application.Abstractions.Database;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Postgres.Database;

public class DbConnectionFactory(DirectoryServiceDbContext dbContext) : IDbConnectionFactory
{
    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = dbContext.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        return connection;
    }
}