using System;
using System.Threading;
using System.Threading.Tasks;
using ConcurrencyApi.Domain.Entities;
using ConcurrencyApi.Infrastructure.Persistence;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace ConcurrencyApi.Features.Categories.Create;

public class CreateCategoryEndpoint : Endpoint<CreateCategoryRequest, CreateCategoryResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateCategoryEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/api/categories");
        AllowAnonymous();
        Description(x => x
            .Produces<CreateCategoryResponse>(201)
            .ProducesProblem(400));
    }

    public override async Task HandleAsync(CreateCategoryRequest req, CancellationToken ct)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Description = req.Description,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Categories.AddAsync(category, ct);
        await _dbContext.SaveChangesAsync(ct);

        var response = new CreateCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.CreatedAt,
            RowVersion = category.RowVersion
        };

        await SendAsync(response, 201, ct);
    }
}
