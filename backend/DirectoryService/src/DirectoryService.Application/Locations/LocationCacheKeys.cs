using System.Security.Cryptography;
using System.Text;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Locations.Requests;

namespace DirectoryService.Application.Locations;

public static class LocationCacheKeys
{
    public const string CollectionPrefix = "locations:list";

    private const string Version = "v1";

    public static class Tags
    {
        public const string All = CollectionPrefix;
        public const string Active = $"{CollectionPrefix}:active";
        public const string Inactive = $"{CollectionPrefix}:inactive";

        public static string Department(Guid departmentId)
            => $"{CollectionPrefix}:dept:{departmentId}";
    }

    public static string BuildListKey(
        bool? isActive,
        Guid[]? departmentIds,
        string? sortBy,
        string? sortDirection,
        int page,
        int pageSize)
    {
        string canonical = string.Join(
            '\x1f',
            isActive?.ToString() ?? "null",
            Normalize(sortBy),
            Normalize(sortDirection),
            page.ToString(),
            pageSize.ToString(),
            Normalize(departmentIds));

        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))[..32].ToLowerInvariant();
        return $"{CollectionPrefix}:{Version}:{hash}";
    }

    public static string BuildListKey(GetLocationsRequest request)
    {
        var pagination = request.Pagination;
        return BuildListKey(
            request.IsActive,
            request.DepartmentIds,
            request.SortBy,
            request.SortDirection,
            pagination?.Page ?? new PaginationRequest().Page,
            pagination?.PageSize ?? new PaginationRequest().PageSize);
    }

    public const string ItemPrefix = "locations:item";

    public static string BuildItemKey(Guid locationId)
        => $"{ItemPrefix}:{Version}:{locationId}";

    public static IEnumerable<string> BuildTags(GetLocationsRequest request)
    {
        var tags = new List<string> { Tags.All };

        if (request.IsActive is null or true)
        {
            tags.Add(Tags.Active);
        }

        if (request.IsActive is null or false)
        {
            tags.Add(Tags.Inactive);
        }

        if (request.DepartmentIds is { Length: > 0 } ids)
        {
            tags.AddRange(ids.Select(Tags.Department));
        }

        return tags.Distinct();
    }

    private static string Normalize(string? value)
        => value?.Trim().ToLowerInvariant() ?? string.Empty;

    private static string Normalize(Guid[]? values)
        => values is { Length: > 0 }
            ? string.Join(',', values.OrderBy(v => v))
            : string.Empty;
}
