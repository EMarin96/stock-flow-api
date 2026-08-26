using StockFlow.Application.Common.Persistence;
using StockFlow.Application.Common.Security;
using StockFlow.Application.Users.Shared;
using StockFlow.Domain.Users;

namespace StockFlow.Api.Startup;

public static class AdminBootstrapExtensions
{
    /// <summary>
    /// Startup admin bootstrap (unconditional — real environments apply
    /// migrations separately, so the Users table already exists by the time this
    /// runs): if the Users table is empty, seed one Admin user from
    /// Seed:AdminUsername/Seed:AdminPassword, or fail fast if that configuration
    /// is missing — otherwise the API would start with no way to log in (see
    /// spec.md — Bootstrap).
    /// </summary>
    public static async Task SeedInitialAdminUserAsync(this WebApplication app)
    {
        using var seedScope = app.Services.CreateScope();

        var userWriteRepository = seedScope.ServiceProvider.GetRequiredService<IUserWriteRepository>();
        var existingUserCount = await userWriteRepository.CountAllAsync(CancellationToken.None);

        if (existingUserCount == 0)
        {
            var adminUsername = app.Configuration["Seed:AdminUsername"];
            var adminPassword = app.Configuration["Seed:AdminPassword"];

            if (string.IsNullOrWhiteSpace(adminUsername) || string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "The Users table is empty and 'Seed:AdminUsername'/'Seed:AdminPassword' are not configured. " +
                    "Set them so an initial Admin user can be created; otherwise there is no way to log in.");
            }

            var passwordHasher = seedScope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var passwordHash = passwordHasher.Hash(adminPassword);

            // createdBy: null — no authenticated actor exists yet at this point.
            var adminUser = User.Create(adminUsername, passwordHash, Role.Admin, createdBy: null);
            await userWriteRepository.AddAsync(adminUser, CancellationToken.None);

            var unitOfWork = seedScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await unitOfWork.SaveChangesAsync(CancellationToken.None);
        }
    }
}
