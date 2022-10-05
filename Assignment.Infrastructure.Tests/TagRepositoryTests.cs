namespace Assignment.Infrastructure.Tests;

public class TagRepositoryTests
{
    private readonly KanbanContext _context;
    private readonly TagRepository _repository;

    private readonly SqliteConnection _connection;
    public TagRepositoryTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(_connection);
        var context = new KanbanContext(builder.Options);
        context.Database.EnsureCreated();
        var User1 = new Tag("Test") { Id = 1 };
        var User2 = new Tag("Test2") { Id = 2 };
        var User3 = new Tag("Test3") { Id = 3 };
        context.Tags.AddRange(User1, User2, User3);
        context.SaveChanges();
        _context = context;
        _repository = new TagRepository(_context);
    }
}
