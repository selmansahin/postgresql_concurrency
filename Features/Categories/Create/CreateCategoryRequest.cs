using System.ComponentModel.DataAnnotations;

namespace ConcurrencyApi.Features.Categories.Create;

public class CreateCategoryRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}
