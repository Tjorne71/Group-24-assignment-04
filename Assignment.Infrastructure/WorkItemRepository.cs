namespace Assignment.Infrastructure;

public class WorkItemRepository : IWorkItemRepository
{
    private readonly KanbanContext _context;

    public WorkItemRepository(KanbanContext context)
    {
        _context = context;
    }
    public (Response Response, int ItemId) Create(WorkItemCreateDTO item)
    {
        var entity = _context.Items.FirstOrDefault(w => w.Title == item.Title);
        Response response;
        if (entity is null) {
            entity = new WorkItem(item.Title);
            entity.Description = item.Description;
            entity.State = State.New;
            entity.Created = DateTime.UtcNow;
            entity.StateUpdated = DateTime.UtcNow;
            entity.Tags = CreateOrUpdateTags(item.Tags).ToHashSet();
            _context.Items.Add(entity);
            _context.SaveChanges();
            response = Response.Created;
        } else {
            response = Response.Conflict;
        }
        return (response, entity.Id);
    }

    public Response Delete(int itemId)
    {
        var entity = _context.Items.FirstOrDefault(w => w.Id == itemId);
        if(entity is null) return Response.Conflict;
        Response response;
        switch(entity.State) {
                case State.New:
                    _context.Items.Remove(entity);
                    _context.SaveChanges();
                    response = Response.Deleted;
                    break;
                case State.Active:
                    entity.UpdateState(State.Removed);
                    _context.SaveChanges();
                    response = Response.Updated;
                    break;
                case State.Resolved:
                case State.Closed:
                case State.Removed:
                    response = Response.Conflict;
                    break;
                default:
                    response = Response.Conflict;
            break;
        }
        return response;
    }

    public WorkItemDetailsDTO? Find(int itemId)
    {
        var item = 
            from w in _context.Items
            let tags = w.Tags.Select(t => t.Name).ToHashSet()
            where w.Id == itemId
            select new WorkItemDetailsDTO(
                w.Id, 
                w.Title, 
                w.Description!, 
                w.Created, 
                w.AssignedTo!.Name, 
                tags, 
                w.State, 
                w.StateUpdated);
        return item.FirstOrDefault();
    }

    public IReadOnlyCollection<WorkItemDTO> Read()
    {
        var items = 
            from w in _context.Items
            let tags = w.Tags.Select(t => t.Name).ToHashSet()
            orderby w.Title
            select new WorkItemDTO(
                w.Id, 
                w.Title, 
                w.AssignedTo!.Name,
                tags,
                w.State
            );

        return items.ToArray();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByState(State state)
    {
        var items = 
            from w in _context.Items
            let tags = w.Tags.Select(t => t.Name).ToHashSet()
            where w.State == state
            orderby w.Title
            select new WorkItemDTO(
                w.Id, 
                w.Title, 
                w.AssignedTo!.Name,
                tags,
                w.State
            );

        return items.ToArray();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByTag(string tag)
    {
        var items = 
            from w in _context.Items
            let tags = w.Tags.Select(t => t.Name).ToHashSet()
            where tags.Contains(tag)
            orderby w.Title
            select new WorkItemDTO(
                w.Id, 
                w.Title, 
                w.AssignedTo!.Name,
                tags,
                w.State
            );

        return items.ToArray();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByUser(int userId)
    {
        var items = 
            from w in _context.Items
            let tags = w.Tags.Select(t => t.Name).ToHashSet()
            where w.AssignedToId == userId
            orderby w.Title
            select new WorkItemDTO(
                w.Id, 
                w.Title, 
                w.AssignedTo!.Name,
                tags,
                w.State
            );

        return items.ToArray();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadRemoved()
    {
        return ReadByState(State.Removed);
    }

    public Response Update(WorkItemUpdateDTO item)
    {
        var entity = _context.Items.FirstOrDefault(w => w.Id == item.Id);
        if(entity is null) return Response.BadRequest;
        var user = FindUserById(item.AssignedToId);
        if(user == null) return Response.BadRequest;
        entity.AssignedTo = user;
        entity.AssignedToId = user.Id;
        entity.Title = item.Title;
        entity.Description = item.Description;
        entity.State = item.State;
        entity.StateUpdated = DateTime.UtcNow;
        entity.Tags = CreateOrUpdateTags(item.Tags).ToHashSet();
        try
        {
            _context.SaveChanges();
            return Response.Updated;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return Response.Conflict;
        }
    }

    private User? FindUserById(int? userId) {
        if(userId == null) return null;
        var user = from u in _context.Users
                where u.Id == userId
                select u;
        return user.FirstOrDefault();
    }

    private Tag? FindTagByName(string? tagName) {
        if(tagName == null) return null;
        var tag = from t in _context.Tags
                where t.Name == tagName
                select t;
        return tag.FirstOrDefault();
    }

    private IEnumerable<Tag> CreateOrUpdateTags(IEnumerable<string> tagNames) {
        var existing = _context.Tags.Where(t => tagNames.Contains(t.Name)).ToDictionary(p => p.Name);

        foreach (var tagName in tagNames)
        {
            existing.TryGetValue(tagName, out var tag);

            yield return tag ?? new Tag(tagName);
        }
    }
}
