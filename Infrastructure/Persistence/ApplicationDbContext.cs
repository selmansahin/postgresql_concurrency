using System;
using System.Threading;
using System.Threading.Tasks;
using ConcurrencyApi.Domain.Entities;
using ConcurrencyApi.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ConcurrencyApi.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        try
        {
            return base.SaveChanges();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle concurrency exception
            HandleConcurrencyException(ex);
            throw;
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle concurrency exception
            HandleConcurrencyException(ex);
            throw;
        }
    }

    private void HandleConcurrencyException(DbUpdateConcurrencyException ex)
    {
        foreach (var entry in ex.Entries)
        {
            var proposedValues = entry.CurrentValues;
            var databaseValues = entry.GetDatabaseValues();

            if (databaseValues == null)
            {
                // The entity was deleted by another user
                throw new ConcurrencyException($"The record was deleted by another user.");
            }

            // Create a custom exception with both current and database values
            var concurrencyEx = new ConcurrencyException(
                $"The record you attempted to edit was modified by another user after you got the original value.");
            
            concurrencyEx.CurrentEntity = entry.Entity;
            var dbValues = entry.GetDatabaseValues();
            if (dbValues != null)
            {
                concurrencyEx.DatabaseEntity = dbValues.ToObject();
            }
            
            throw concurrencyEx;
        }
    }
}
