using System.Collections.Generic;
using ConcurrencyApi.Features.Categories;

namespace ConcurrencyApi.Features.Categories.Get;

public class GetCategoriesResponse
{
    public List<CategoryDto> Categories { get; set; } = new();
}
