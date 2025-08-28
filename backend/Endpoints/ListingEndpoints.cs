using Octopets.Backend.Models;
using Octopets.Backend.Repositories.Interfaces;

namespace Octopets.Backend.Endpoints;

public static class ListingEndpoints
{    // Method to simulate expensive operation without memory exhaustion
    private static void AReallyExpensiveOperation()
    {
        // Simulate expensive CPU work instead of memory allocation to prevent OOM
        // This provides the same delay/expensive operation effect without memory risk
        var random = new Random();
        double result = 0;
        
        // Perform CPU-intensive calculations for approximately 1 second
        for (int i = 0; i < 1000000; i++)
        {
            // Complex mathematical operations that consume CPU time
            result += Math.Sqrt(random.NextDouble() * 1000) * Math.Sin(i) * Math.Cos(i);
            
            // Add small delays periodically to make the operation visible
            if (i % 100000 == 0)
            {
                Thread.Sleep(10);
            }
        }
        
        // Prevent compiler optimization by using the result
        _ = result;
    }

    public static void MapListingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/listings")
                       .WithTags("Listings");

        // GET all listings
        group.MapGet("/", async (IListingRepository repository) =>
        {
            var listings = await repository.GetAllAsync();
            return Results.Ok(listings);
        })
        .WithName("GetAllListings")
        .WithDescription("Gets all listings")
        .WithOpenApi();        // GET listing by id
        group.MapGet("/{id:int}", async (int id, IListingRepository repository, IConfiguration config) =>
        {
            // Only throw exception or simulate memory issues if ERRORS flag is set to true
            if (config.GetValue<bool>("ERRORS"))
            {
                AReallyExpensiveOperation();
            }

            var listing = await repository.GetByIdAsync(id);
            return listing is null ? Results.NotFound() : Results.Ok(listing);
        })
        .WithName("GetListingById")
        .WithDescription("Gets a listing by its ID")
        .WithOpenApi(operation =>
        {
            operation.Parameters[0].Description = "The ID of the listing";
            return operation;
        });        // POST new listing
        group.MapPost("/", async (Listing listing, IListingRepository repository, IConfiguration config) =>
        {
            if (!config.GetValue<bool>("ENABLE_CRUD", true))
            {
                throw new InvalidOperationException("CRUD operations are currently disabled");
            }
            var newListing = await repository.CreateAsync(listing);
            return Results.Created($"/api/listings/{newListing.Id}", newListing);
        })
        .WithName("CreateListing")
        .WithDescription("Creates a new listing")
        .WithOpenApi();

        // PUT update listing
        group.MapPut("/{id:int}", async (int id, Listing listing, IListingRepository repository, IConfiguration config) =>
        {
            if (!config.GetValue<bool>("ENABLE_CRUD", true))
            {
                throw new InvalidOperationException("CRUD operations are currently disabled");
            }
            var updatedListing = await repository.UpdateAsync(id, listing);
            return updatedListing is null ? Results.NotFound() : Results.Ok(updatedListing);
        })
        .WithName("UpdateListing")
        .WithDescription("Updates an existing listing")
        .WithOpenApi();

        // DELETE listing
        group.MapDelete("/{id:int}", async (int id, IListingRepository repository, IConfiguration config) =>
        {
            if (!config.GetValue<bool>("ENABLE_CRUD", true))
            {
                throw new InvalidOperationException("CRUD operations are currently disabled");
            }
            var result = await repository.DeleteAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteListing")
        .WithDescription("Deletes a listing")
        .WithOpenApi();
    }
}
