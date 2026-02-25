namespace SampleBookStoreApi.Dtos;

public sealed record BookResponseDto(
    Guid Id,
    string Title,
    string Author,
    int Year,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    IReadOnlyList<LinkDto> Links
);