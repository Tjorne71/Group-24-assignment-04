

namespace Assignment.Infrastructure.Tests;

public class UserRepositoryTests : IDisposable
{
    private readonly KanbanContext _context;
    private readonly UserRepository _repository;

    private readonly SqliteConnection _connection;
    public UserRepositoryTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(_connection);
        var context = new KanbanContext(builder.Options);
        context.Database.EnsureCreated();
        var User1 = new User("Test", "test@test.dk") { Id = 1 };
        var User2 = new User("Test2", "test2@test.dk") { Id = 2 };
        var User3 = new User("Test3", "test3@test.dk") { Id = 3 };
        context.Users.AddRange(User1, User2, User3);
        context.SaveChanges();

        _context = context;
        _repository = new UserRepository(_context);
    }

    [Fact]
    public void Creating_Uniq_User_Returns_Created()
    {
        // Given
        var newUser = new UserCreateDTO("Test4", "test4@test.dk");
        // When
        var (response, userCreated) = _repository.Create(newUser);
        // Then
        response.Should().Be(Response.Created);
        userCreated.Should().Be(4);
    }

    [Fact]
    public void Creating_Existing_User_Returns_Conflict()
    {
        // Given
        var newUser = new UserCreateDTO("Test", "test@test.dk");
        // When
        var (response, userCreated) = _repository.Create(newUser);
        // Then
        response.Should().Be(Response.Conflict);
        userCreated.Should().Be(1);
    }

    [Fact]
    public void Creating_User_With_Existing_Email_Returns_Conflict()
    {
        // Given
        var newUser = new UserCreateDTO("Test4", "test@test.dk");
        // When
        var (response, userCreated) = _repository.Create(newUser);
        // Then
        response.Should().Be(Response.Conflict);
        userCreated.Should().Be(1);
    }

    [Fact]
    public void Deleting_Existing_User_Returns_Deleted()
    {
        // Given
        var activeUser = new User("activeUser", "active@active.dk") { Id = 4 };
        _context.Users.Add(activeUser);
        _context.SaveChanges();
        // When
        var response = _repository.Delete(4);
        // Then
        response.Should().Be(Response.Deleted);
    }

    [Fact]
    public void Deleting_Existing_User_Assigned_To_WorkItem_Without_Force_Returns_Conflict()
    {
        // Given
        var activeUser = new User("activeUser", "active@active.dk") { Id = 4 };
        var workItem = new WorkItem("someWorkItem");
        activeUser.Items.Add(workItem);
        _context.Users.Add(activeUser);
        _context.SaveChanges();
        // When
        var response = _repository.Delete(4);
        // Then
        response.Should().Be(Response.Conflict);
    }

    [Fact]
    public void Deleting_Existing_User_Assigned_To_WorkItem_With_Force_Returns_Deleted()
    {
        // Given
        var activeUser = new User("activeUser", "active@active.dk") { Id = 4 };
        var workItem = new WorkItem("someWorkItem");
        activeUser.Items.Add(workItem);
        _context.Users.Add(activeUser);
        _context.SaveChanges();
        // When
        var response = _repository.Delete(4, true);
        // Then
        response.Should().Be(Response.Deleted);
    }

    [Fact]
    public void Searching_Existing_User_Returns_Match()
    {
        // Given
        var userId = 1;
        // When
        var result = _repository.Find(userId);
        // Then
        result.Should().Be(new UserDTO(1, "Test", "test@test.dk"));
    }

    [Fact]
    public void Searching_Non_Existing_User_Returns_Null()
    {
        // Given
        var userId = 4;
        // When
        var result = _repository.Find(userId);
        // Then
        result.Should().Be(null);
    }

    [Fact]
    public void Updating_Non_Existing_User_Returns_NotFound()
    {
        // Given
        var userUpdate = new UserUpdateDTO(5, "newTestName", "newtest@test.dk");
        // When
        var result = _repository.Update(userUpdate);
        // Then
        result.Should().Be(Response.NotFound);
    }

    [Fact]
    public void Updating_Existing_User_Returns_Updated()
    {
        // Given
        var userUpdate = new UserUpdateDTO(1, "newTestName", "newtest@test.dk");
        // When
        var result = _repository.Update(userUpdate);
        // Then
        result.Should().Be(Response.Updated);
    }

    [Fact]
    public void Updating_Existing_Users_Email_To_Existing_Email_Returns_Conflict()
    {
        // Given
        var userUpdate = new UserUpdateDTO(1, "newTestName", "test2@test.dk");
        // When
        var result = _repository.Update(userUpdate);
        // Then
        result.Should().Be(Response.Conflict);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
