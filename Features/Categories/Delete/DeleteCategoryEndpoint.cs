using System.Threading;
using System.Threading.Tasks;
using ConcurrencyApi.Domain.Exceptions;
using ConcurrencyApi.Infrastructure.Persistence;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace ConcurrencyApi.Features.Categories.Delete;

public class DeleteCategoryEndpoint : Endpoint<DeleteCategoryRequest, DeleteCategoryResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public DeleteCategoryEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Delete("/api/categories/{Id}");
        AllowAnonymous();
        Description(x => x
            .Produces<DeleteCategoryResponse>(200)
            .ProducesProblem(404)
            .ProducesProblem(409)); // 409 Conflict for concurrency issues
    }

    public override async Task HandleAsync(DeleteCategoryRequest req, CancellationToken ct)
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
            AddError("The entity has been modified by another process.");
            AddError($"Current version: {category.RowVersion}, Requested version: {req.RowVersion}");
            await SendErrorsAsync(409, ct);
            return;
        }

        // Remove the entity
        _dbContext.Categories.Remove(category);

        try
        {
            await _dbContext.SaveChangesAsync(ct);

            var response = new DeleteCategoryResponse
            {
                Id = req.Id,
                Success = true,
                Message = "Category successfully deleted."
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
