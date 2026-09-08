using Microsoft.Extensions.Logging;

namespace FileService.Application.Features.Messaging;

public class DepartmentDeletedEventHandler
{
    public static async Task Handle(
        ILogger<DepartmentDeletedEventHandler> logger,
        CancellationToken cancellationToken)
    {
    }
}