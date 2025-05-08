using System;

namespace ConcurrencyApi.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public uint RowVersion { get; set; } // This will be mapped to PostgreSQL's xmin system column
}
