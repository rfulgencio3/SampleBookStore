using Microsoft.EntityFrameworkCore;
using SampleBookStoreApi.Models;

namespace SampleBookStoreApi.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();
}