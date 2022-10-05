namespace Assignment.Infrastructure;

public class TagRepository : ITagRepository
{
    private readonly KanbanContext _context;

    public TagRepository(KanbanContext context)
    {
        _context = context;
    }
    public (Response Response, int TagId) Create(TagCreateDTO tag)
    {
        var entity = _context.Tags.FirstOrDefault(t => t.Name == tag.Name);
        Response response;
        if(entity is null) {
            entity = new Tag(tag.Name);
            _context.Tags.Add(entity);
            _context.SaveChanges();
            response = Response.Created;
        } else {
            response = Response.Conflict;
        }
        return (response, entity.Id);
    }

    public Response Delete(int tagId, bool force = false)
    {
        var entity = _context.Tags.FirstOrDefault(t => t.Id == tagId);
        Response response;
        if(entity != null && (entity.WorkItems.Count == 0 || force)) {
            _context.Tags.Remove(entity);
            _context.SaveChanges();
            response = Response.Deleted;
        } else {
            response = Response.Conflict;
        }
        return response;
    }

    public TagDTO? Find(int tagId)
    {
        var tag = 
            from t in _context.Tags
            where t.Id == tagId
            select new TagDTO(t.Id, t.Name);
        return tag.FirstOrDefault();
    }

    public IReadOnlyCollection<TagDTO> Read()
    {
        var tags = 
            from t in _context.Tags
            orderby t.Name
            select new TagDTO(t.Id, t.Name);

        return tags.ToArray();
    }

    public Response Update(TagUpdateDTO tag)
    {
        var entity = _context.Tags.FirstOrDefault(t => t.Id == tag.Id);
        Response response;
        if(entity == null) {
            response = Response.NotFound;
        } else {
            entity.Name = tag.Name;
            try
            {
                _context.SaveChanges();
                response = Response.Updated;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                response = Response.Conflict;
            }
        }
        return response;
    }
}
