namespace SampleBookStoreApi.Dtos;

public sealed record BookCreateDto(string Title, string Author, int Year);