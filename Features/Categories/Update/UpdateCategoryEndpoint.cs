using System;
using System.Threading;
using System.Threading.Tasks;
using ConcurrencyApi.Domain.Exceptions;
using ConcurrencyApi.Infrastructure.Persistence;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace ConcurrencyApi.Features.Categories.Update;

public class UpdateCategoryEndpoint : Endpoint<UpdateCategoryRequest, UpdateCategoryResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateCategoryEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Put("/api/categories/{Id}");
        AllowAnonymous();
        Description(x => x
            .Produces<UpdateCategoryResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409)); // 409 Conflict for concurrency issues
    }

    public override async Task HandleAsync(UpdateCategoryRequest req, CancellationToken ct)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == req.Id, ct);

        if (category == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        // Check if the RowVersion matches
        if (category.RowVersion != req.RowVersion)
        {
            // The entity has been modified since it was retrieved
            var currentValues = new
            {
                category.Name,
                category.Description,
                category.RowVersion
            };

            AddError("The entity has been modified by another process.");
            AddError($"Current version: {category.RowVersion}, Requested version: {req.RowVersion}");
            await SendErrorsAsync(409, ct);
            return;
        }

        // Update the entity
        category.Name = req.Name;
        category.Description = req.Description;
        category.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _dbContext.SaveChangesAsync(ct);

            var response = new UpdateCategoryResponse
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
        catch (ConcurrencyException ex)
        {
            AddError(ex.Message);
            await SendErrorsAsync(409, ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            AddError("The entity has been modified by another process.");
            await SendErrorsAsync(409, ct);
        }
    }
}
