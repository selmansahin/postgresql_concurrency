using System;

namespace ConcurrencyApi.Features.Categories.Create;

public class CreateCategoryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public uint RowVersion { get; set; }
}
