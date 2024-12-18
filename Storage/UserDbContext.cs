using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Storage;

public record UserRecord(Guid Id, string Email, DateTime CreatedAt);

public class UserRecordValidator : AbstractValidator<UserRecord>
{
    public UserRecordValidator()
    {
        RuleFor(x => x.Id).NotNull();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.CreatedAt).NotNull();
    }
}

#pragma warning disable CS8618

public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<UserRecord> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<UserRecord>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Email).IsRequired();
            entity.Property(r => r.CreatedAt).IsRequired();
        });
    }
}

#pragma warning restore CS8618
