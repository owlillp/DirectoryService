using System.Data;
using Core.Abstractions.Database;
using Microsoft.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres.Database;

public class DbConnectionFactory(FileServiceDbContext dbContext) : IDbConnectionFactory
{
    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = dbContext.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        return connection;
    }
}