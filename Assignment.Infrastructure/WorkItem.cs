namespace Assignment.Infrastructure;

public class WorkItem
{
    public int Id { get; set; }

    public string Title { get; set; }

    public int? AssignedToId { get; set; }

    public User? AssignedTo { get; set; }

    public State State { get; set; }

    public DateTime StateUpdated { get; set; }

    public DateTime Created { get; set; }

    public String? Description { get; set; }

    public ICollection<Tag> Tags { get; set; }

    public WorkItem(string title)
    {
        Title = title;
        Tags = new HashSet<Tag>();
        Created = DateTime.UtcNow;
        State = State.New;
    }

    public void UpdateState(State state) { 
        State = state;
        StateUpdated = DateTime.UtcNow;
    }
}
