using Microsoft.EntityFrameworkCore;

namespace Storage.Tests;

public class UserDbContextTests
{
    private static DbContextOptions<UserDbContext> GetInMemoryDbContextOptions() =>
        new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(Ulid.NewUlid().ToString())
            .Options;

    [Fact]
    public async Task AddUser_ShouldAddUserToDatabase()
    {
        // Arrange
        using var context = new UserDbContext(GetInMemoryDbContextOptions());

        var user = new UserRecord(Guid.NewGuid(), "test@example.com", DateTime.UtcNow);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var storedUser = await context.Users.FirstOrDefaultAsync(u =>
            u.Email == "test@example.com"
        );

        Assert.NotNull(storedUser);
        Assert.Equal(user.Email, storedUser.Email);
        Assert.Equal(user.CreatedAt, storedUser.CreatedAt);
    }

    [Fact]
    public async Task DeleteUser_ShouldRemoveUserFromDatabase()
    {
        // Arrange
        using var context = new UserDbContext(GetInMemoryDbContextOptions());
        var user = new UserRecord(Guid.NewGuid(), "test@example.com", DateTime.UtcNow);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        context.Users.Remove(user);
        await context.SaveChangesAsync();

        // Assert
        var storedUser = await context.Users.FirstOrDefaultAsync(u =>
            u.Email == "test@example.com"
        );
        Assert.Null(storedUser);
    }
}
