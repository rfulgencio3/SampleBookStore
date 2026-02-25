using Microsoft.EntityFrameworkCore;
using SampleBookStoreApi.Data;
using SampleBookStoreApi.Dtos;
using SampleBookStoreApi.Dtos.Responses;
using SampleBookStoreApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core InMemoryDatabase
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("SampleBookStoreDb"));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "";
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SampleBookStoreApi v1");
});

// Seed (opcional, para testar rapidamente)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Books.Any())
    {
        db.Books.AddRange(
            new Book { Title = "Clean Architecture", Author = "Robert C. Martin", Year = 2017 },
            new Book { Title = "Domain-Driven Design", Author = "Eric Evans", Year = 2003 },
            new Book { Title = "Refactoring", Author = "Martin Fowler", Year = 1999 }
        );
        db.SaveChanges();
    }
}

static IReadOnlyList<LinkDto> BookLinks(HttpContext http, Guid id) => new[]
{
    new LinkDto("self",   $"{http.Request.Scheme}://{http.Request.Host}/api/books/{id}", "GET"),
    new LinkDto("update", $"{http.Request.Scheme}://{http.Request.Host}/api/books/{id}", "PUT"),
    new LinkDto("delete", $"{http.Request.Scheme}://{http.Request.Host}/api/books/{id}", "DELETE"),
};

static IReadOnlyList<LinkDto> CollectionLinks(HttpContext http, int page, int pageSize, int totalPages)
{
    var baseUrl = $"{http.Request.Scheme}://{http.Request.Host}/api/books";
    var links = new List<LinkDto>
    {
        new("self", $"{baseUrl}?page={page}&pageSize={pageSize}", "GET"),
        new("create", $"{baseUrl}", "POST")
    };

    if (page > 1)
        links.Add(new("prev", $"{baseUrl}?page={page - 1}&pageSize={pageSize}", "GET"));

    if (page < totalPages)
        links.Add(new("next", $"{baseUrl}?page={page + 1}&pageSize={pageSize}", "GET"));

    return links;
}

static BookResponseDto MapToDto(HttpContext http, Book b) =>
    new(b.Id, b.Title, b.Author, b.Year, b.CreatedAtUtc, b.UpdatedAtUtc, BookLinks(http, b.Id));

var group = app.MapGroup("/api/books").WithTags("Books");

// GET /api/books?page=1&pageSize=10  (lista paginada)
group.MapGet("/", async (HttpContext http, AppDbContext db, int page = 1, int pageSize = 10) =>
{
    page = page < 1 ? 1 : page;
    pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

    var totalCount = await db.Books.CountAsync();
    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    if (totalPages == 0) totalPages = 1;

    if (page > totalPages) page = totalPages;

    var items = await db.Books
        .AsNoTracking()
        .OrderBy(b => b.Title)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    var dtoItems = items.Select(b => MapToDto(http, b)).ToList();

    var response = new PagedResponse<BookResponseDto>(
        Items: dtoItems,
        Page: page,
        PageSize: pageSize,
        TotalCount: totalCount,
        TotalPages: totalPages,
        Links: CollectionLinks(http, page, pageSize, totalPages)
    );

    return Results.Ok(response);
})
.WithName("GetBooks");

// GET /api/books/{id} (busca por id)
group.MapGet("/{id:guid}", async (HttpContext http, AppDbContext db, Guid id) =>
{
    var book = await db.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
    return book is null ? Results.NotFound() : Results.Ok(MapToDto(http, book));
})
.WithName("GetBookById");

// POST /api/books (criação)
group.MapPost("/", async (HttpContext http, AppDbContext db, BookCreateDto input) =>
{
    if (string.IsNullOrWhiteSpace(input.Title))
        return Results.BadRequest(new { error = "Title is required." });

    if (string.IsNullOrWhiteSpace(input.Author))
        return Results.BadRequest(new { error = "Author is required." });

    var entity = new Book
    {
        Id = Guid.NewGuid(),
        Title = input.Title.Trim(),
        Author = input.Author.Trim(),
        Year = input.Year,
        CreatedAtUtc = DateTime.UtcNow
    };

    db.Books.Add(entity);
    await db.SaveChangesAsync();

    var dto = MapToDto(http, entity);
    return Results.Created($"/api/books/{entity.Id}", dto);
})
.WithName("CreateBook");

// PUT /api/books/{id} (alteração por id)
group.MapPut("/{id:guid}", async (HttpContext http, AppDbContext db, Guid id, BookUpdateDto input) =>
{
    var entity = await db.Books.FirstOrDefaultAsync(b => b.Id == id);
    if (entity is null) return Results.NotFound();

    if (string.IsNullOrWhiteSpace(input.Title))
        return Results.BadRequest(new { error = "Title is required." });

    if (string.IsNullOrWhiteSpace(input.Author))
        return Results.BadRequest(new { error = "Author is required." });

    entity.Title = input.Title.Trim();
    entity.Author = input.Author.Trim();
    entity.Year = input.Year;
    entity.UpdatedAtUtc = DateTime.UtcNow;

    await db.SaveChangesAsync();

    // (Opcional) retornar o recurso atualizado com HATEOAS
    return Results.Ok(MapToDto(http, entity));
})
.WithName("UpdateBook");

// DELETE /api/books/{id} (deleção por id)
group.MapDelete("/{id:guid}", async (AppDbContext db, Guid id) =>
{
    var entity = await db.Books.FirstOrDefaultAsync(b => b.Id == id);
    if (entity is null) return Results.NotFound();

    db.Books.Remove(entity);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("DeleteBook");

app.Run();