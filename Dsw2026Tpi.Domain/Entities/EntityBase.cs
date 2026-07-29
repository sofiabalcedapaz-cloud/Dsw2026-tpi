namespace Dsw2026Tpi.Domain.Entities;

public abstract class EntityBase(Guid? id = null)
{
    public Guid Id { get; init; } = id ?? Guid.NewGuid();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool Deleted { get; set; } = false;  
}
