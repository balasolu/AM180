﻿namespace AM180.Models.Interfaces;

public interface IEntity<TId>
    where TId : IEquatable<TId>
{
    public TId? Id { get; set; }
    public long? CreatedAt { get; set; }
}
