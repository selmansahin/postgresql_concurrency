using System;

namespace ConcurrencyApi.Features.Categories.GetById;

public class GetCategoryByIdResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public uint RowVersion { get; set; }
}
