using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConcurrencyApi.Features.Categories;
using ConcurrencyApi.Infrastructure.Persistence;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace ConcurrencyApi.Features.Categories.Get;

public class GetCategoriesEndpoint : EndpointWithoutRequest<GetCategoriesResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetCategoriesEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/api/categories");
        AllowAnonymous();
        Description(x => x.Produces<GetCategoriesResponse>(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var categories = await _dbContext.Categories
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                RowVersion = c.RowVersion
            })
            .ToListAsync(ct);

        var response = new GetCategoriesResponse
        {
            Categories = categories
        };

        await SendOkAsync(response, ct);
    }
}
