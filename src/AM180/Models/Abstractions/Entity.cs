using AM180.Models.Interfaces;

namespace AM180.Models.Abstractions;

public abstract class Entity<TId> : IEntity<TId>
    where TId : IEquatable<TId>
{
    public TId? Id { get; set; }
    public long? CreatedAt { get; set; }
}
