namespace SampleBookStoreApi.Dtos.Responses;

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    IReadOnlyList<LinkDto> Links
);