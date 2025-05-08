using System;
using System.ComponentModel.DataAnnotations;

namespace ConcurrencyApi.Features.Categories.Delete;

public class DeleteCategoryRequest
{
    [Required]
    public Guid Id { get; set; }
    
    [Required]
    public uint RowVersion { get; set; } // Concurrency control için gerekli
}
