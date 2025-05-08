using System.Threading;
using System.Threading.Tasks;
using ConcurrencyApi.Infrastructure.Persistence;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace ConcurrencyApi.Features.Categories.GetById;

public class GetCategoryByIdEndpoint : Endpoint<GetCategoryByIdRequest, GetCategoryByIdResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetCategoryByIdEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/api/categories/{Id}");
        AllowAnonymous();
        Description(x => x
            .Produces<GetCategoryByIdResponse>(200)
            .ProducesProblem(404));
    }

    public override async Task HandleAsync(GetCategoryByIdRequest req, CancellationToken ct)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == req.Id, ct);

        if (category == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var response = new GetCategoryByIdResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            RowVersion = category.RowVersion
        };

        await SendOkAsync(response, ct);
    }
}
