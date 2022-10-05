namespace Assignment.Infrastructure;

public class UserRepository : IUserRepository
{
    private readonly KanbanContext _context;

    public UserRepository(KanbanContext context)
    {
        _context = context;
    }
    public (Response Response, int UserId) Create(UserCreateDTO user)
    {
        var entity = _context.Users.FirstOrDefault(u => u.Name == user.Name);
        Response response;
        if(entity is null) {
            entity = new User(user.Name, user.Email);
            try
            {
                _context.Users.Add(entity);
                _context.SaveChanges();
                response = Response.Created;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                entity = _context.Users.FirstOrDefault(u => u.Email == user.Email);
                response = Response.Conflict;
            }
        } else {
            response = Response.Conflict;
        }
        var created = new UserDTO(entity!.Id, entity.Name, entity.Email);
        return (response, created.Id);
    }

    public Response Delete(int userId, bool force = false)
    {
        var entity = _context.Users.FirstOrDefault(u => u.Id == userId);
        Response response;
        if(entity != null && (entity.Items.Count == 0 || force)) {
            _context.Users.Remove(entity);
            response = Response.Deleted;

        } else {
            response = Response.Conflict;
        }

        return response;
    }

    public UserDTO? Find(int userId)
    {
        var entity = _context.Users.FirstOrDefault(u => u.Id == userId);
        if(entity == null) {
            return null;
        }
        return new UserDTO(entity!.Id, entity.Name, entity.Email);
    }

    public IReadOnlyCollection<UserDTO> Read()
    {
        var users = from u in _context.Users
                     orderby u.Name
                     select new UserDTO(u.Id, u.Name, u.Email);

        return users.ToArray();
    }

    public Response Update(UserUpdateDTO user)
    {
        var entity = _context.Users.FirstOrDefault(u => u.Id == user.Id);
        Response response;
        if(entity == null) {
            response = Response.NotFound;
        } else {
            entity.Name = user.Name;
            entity.Email = user.Email;
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
