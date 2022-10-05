namespace Assignment.Infrastructure.Tests;

public class WorkItemRepositoryTests
{
    private readonly KanbanContext _context;
    private readonly WorkItemRepository _repository;

    private readonly SqliteConnection _connection;
    public WorkItemRepositoryTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(_connection);
        var context = new KanbanContext(builder.Options);
        context.Database.EnsureCreated();
        var WorkItem1 = new WorkItem("Test") { Id = 1 };
        var WorkItem2 = new WorkItem("Test2") { Id = 2 };
        var WorkItem3 = new WorkItem("Test3") { Id = 3 };
        context.Items.AddRange(WorkItem1, WorkItem2, WorkItem3);
        context.SaveChanges();

        _context = context;
        _repository = new WorkItemRepository(_context);
    }

    [Fact]
    public void Creating_New_WorkItem_Returns_Created()
    {
        // Given
        var newWorkItem = new WorkItemCreateDTO("Test4" , null, new HashSet<string>());
        // When
        var (response, createdWorkItemId) = _repository.Create(newWorkItem);
        // Then
        response.Should().Be(Response.Created);
        createdWorkItemId.Should().Be(4);
    }

    [Fact]
    public void Creating_New_WorkItem_That_Exist_Returns_Conflict()
    {
        // Given
        var newWorkItem = new WorkItemCreateDTO("Test",  null, new HashSet<string>());
        // When
        var (response, createdWorkItemId) = _repository.Create(newWorkItem);
        // Then
        response.Should().Be(Response.Conflict);
        createdWorkItemId.Should().Be(1);
    }

    [Fact]
    public void Deleting_Existing_WorkItem_With_State_New_Returns_Deleted()
    {
        // Given
        var workItemId = 1;
        // When
        var response = _repository.Delete(workItemId);
        // Then
        response.Should().Be(Response.Deleted);
    }

    [Fact]
    public void Deleting_Existing_WorkItem_With_State_Active_Returns_Updated()
    {
        // Given
        var activeWorkItem = new WorkItem("ActiveTest") { Id = 4 };
        activeWorkItem.UpdateState(State.Active);
        _context.Items.Add(activeWorkItem);
        _context.SaveChanges();
        // When
        var response = _repository.Delete(4);
        // Then
        response.Should().Be(Response.Updated);
        activeWorkItem.State.Should().Be(State.Removed);
    }

    [Fact]
    public void Deleting_Existing_WorkItem_With_State_Closed_Returns_Conflict()
    {
        // Given
        var closedWorkItem = new WorkItem("ClosedTest") { Id = 4 };
        closedWorkItem.UpdateState(State.Closed);
        _context.Items.Add(closedWorkItem);
        _context.SaveChanges();
        // When
        var response = _repository.Delete(4);
        // Then
        response.Should().Be(Response.Conflict);
        closedWorkItem.State.Should().Be(State.Closed);
    }

    [Fact]
    public void Deleting_Existing_WorkItem_With_State_Resolved_Returns_Conflict()
    {
        // Given
        var resolvedWorkItem = new WorkItem("ResolvedTest") { Id = 4 };
        resolvedWorkItem.UpdateState(State.Resolved);
        _context.Items.Add(resolvedWorkItem);
        _context.SaveChanges();
        // When
        var response = _repository.Delete(4);
        // Then
        response.Should().Be(Response.Conflict);
        resolvedWorkItem.State.Should().Be(State.Resolved);
    }

    [Fact]
    public void Deleting_Existing_WorkItem_With_State_Removed_Returns_Conflict()
    {
        // Given
        var removedWorkItem = new WorkItem("RemovedTest") { Id = 4 };
        removedWorkItem.UpdateState(State.Removed);
        _context.Items.Add(removedWorkItem);
        _context.SaveChanges();
        // When
        var response = _repository.Delete(4);
        // Then
        response.Should().Be(Response.Conflict);
        removedWorkItem.State.Should().Be(State.Removed);
    }

    [Fact]
    public void Searching_Existing_WorkItem_Returns_Match()
    {
        // Given
        var workItemId = 1;
        // When
        var result = _repository.Find(workItemId);
        // Then
        result!.Title.Should().Be("Test");
    }

    [Fact]
    public void Searching_Non_Existing_WorkItem_Returns_Null()
    {
        // Given
        var workItemId = 4;
        // When
        var result = _repository.Find(workItemId);
        // Then
        Assert.Null(result);
    }

    [Fact]
    public void Updating_WorkItem_With_Valid_Credential_Returns_Updated()
    {
        // Given
        var user = new User("Assigned User", "assigned@test.dk") {Id = 1};
        _context.Users.Add(user);
        _context.SaveChanges();
        var tag1 = new Tag("tag1") {Id = 1};
        var tag2 = new Tag("tag2") {Id = 2};
        var tagList = new List<String>() {tag1.Name, tag2.Name};
        var updateWorkItem = new WorkItemUpdateDTO(
            1, 
            "Updated",  
            user.Id, 
            "testtest",
            tagList,
            State.Active

        );
        // When
        var response = _repository.Update(updateWorkItem);
        // Then
        response.Should().Be(Response.Updated);
    }

    [Fact]
    public void Updating_WorkItem_Non_Existing_User_Returns_BadRequest()
    {
        // Given
        var user = new User("Assigned User", "assigned@test.dk") {Id = 1};
        var tag1 = new Tag("tag1") {Id = 1};
        var tag2 = new Tag("tag2") {Id = 2};
        var tagList = new List<String>() {tag1.Name, tag2.Name};
        var updateWorkItem = new WorkItemUpdateDTO(
            1, 
            "Updated",  
            user.Id, 
            "testtest",
            tagList,
            State.Active

        );
        // When
        var response = _repository.Update(updateWorkItem);
        // Then
        response.Should().Be(Response.BadRequest);
    }
}
