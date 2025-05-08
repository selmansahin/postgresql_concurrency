using System;
using System.ComponentModel.DataAnnotations;

namespace ConcurrencyApi.Features.Categories.Update;

public class UpdateCategoryRequest
{
    [Required]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public uint RowVersion { get; set; } // This is crucial for concurrency control
}
