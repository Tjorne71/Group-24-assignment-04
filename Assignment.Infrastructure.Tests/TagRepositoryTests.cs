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
        var Tag1 = new Tag("Test") { Id = 1 };
        var Tag2 = new Tag("Test2") { Id = 2 };
        var Tag3 = new Tag("Test3") { Id = 3 };
        context.Tags.AddRange(Tag1, Tag2, Tag3);
        context.SaveChanges();
        _context = context;
        _repository = new TagRepository(_context);
    }

    [Fact]
    public void Creating_New_Tag_Returns_Created()
    {
        // Given
        var newTag = new TagCreateDTO("Test4");
        // When
        var (response, createdTagId) = _repository.Create(newTag);
        // Then
        response.Should().Be(Response.Created);
        createdTagId.Should().Be(4);
    }

    [Fact]
    public void Creating_Existing_Tag_Returns_Conflict()
    {
        // Given
        var newTag = new TagCreateDTO("Test");
        // When
        var (response, createdTagId) = _repository.Create(newTag);
        // Then
        response.Should().Be(Response.Conflict);
        createdTagId.Should().Be(1);
    }

    [Fact]
    public void Deleting_Existing_Tag_Returns_Deleted()
    {
        // Given
        var tagId = 1;
        // When
        var response = _repository.Delete(tagId);
        // Then
        response.Should().Be(Response.Deleted);
    }

    [Fact]
    public void Deleting_Existing_Tag_In_Use_Without_Force_Returns_Conflict()
    {
        // Given
        var activeTag = new Tag("activeTag") { Id = 4 };
        var workItem = new WorkItem("someWorkItem");
        activeTag.WorkItems.Add(workItem);
        _context.Tags.Add(activeTag);
        _context.SaveChanges();
        // When
        var response = _repository.Delete(activeTag.Id);
        // Then
        response.Should().Be(Response.Conflict);
    }

    [Fact]
    public void Deleting_Existing_Tag_In_Use_With_Force_Returns_Conflict()
    {
        // Given
        var activeTag = new Tag("activeTag") { Id = 4 };
        var workItem = new WorkItem("someWorkItem");
        activeTag.WorkItems.Add(workItem);
        _context.Tags.Add(activeTag);
        _context.SaveChanges();
        // When
        var response = _repository.Delete(activeTag.Id, true);
        // Then
        response.Should().Be(Response.Deleted);
    }

    [Fact]
    public void Searching_Existing_Tag_Returns_Match()
    {
        // Given
        var tagId = 1;
        // When
        var response = _repository.Find(tagId);
        // Then
        response.Should().Be(new TagDTO(1, "Test"));
    }

    [Fact]
    public void Searching_Existing_Tag_Returns_Null()
    {
        // Given
        var tagId = 4;
        // When
        var response = _repository.Find(tagId);
        // Then
        response.Should().Be(null);
    }

    [Fact]
    public void Updating_Existing_Tag_Returns_Updated()
    {
        // Given
        var updateTag = new TagUpdateDTO(1, "newTestName");
        // When
        var response = _repository.Update(updateTag);
        // Then
        response.Should().Be(Response.Updated);
    }

    [Fact]
    public void Updating_Existing_Tag_To_Existing_Tag_Name_Returns_Conflict()
    {
        // Given
        var updateTag = new TagUpdateDTO(1, "Test2");
        // When
        var response = _repository.Update(updateTag);
        // Then
        response.Should().Be(Response.Conflict);
    }

    [Fact]
    public void Updating_Non_Existing_Tag_Returns_NotFound()
    {
        // Given
        var updateTag = new TagUpdateDTO(4, "NotATag");
        // When
        var response = _repository.Update(updateTag);
        // Then
        response.Should().Be(Response.NotFound);
    }
    
}
