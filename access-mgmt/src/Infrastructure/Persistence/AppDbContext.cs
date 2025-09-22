using Application.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public sealed class AppDbContext : DbContext, IAppDbContext
    {
        public DbSet<Person> People => Set<Person>();
        public DbSet<Admin> Admins => Set<Admin>();
        public DbSet<User> Users => Set<User>();
        public DbSet<UserGroup> UserGroups => Set<UserGroup>();
        public DbSet<AccessRule> AccessRules => Set<AccessRule>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Person>(e =>
            {
                e.ToTable("People");
                e.HasKey(x => x.Id);
                e.HasDiscriminator<string>("PersonType")
                 .HasValue<Admin>("Admin")
                 .HasValue<User>("User");

                e.HasIndex(x => x.Email).IsUnique();
                e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
                e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
                e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            });

            b.Entity<Admin>(e =>
            {
                e.Property(x => x.Privilege).HasMaxLength(50).IsRequired();
            });

            b.Entity<User>(e =>
            {
                e.HasOne(u => u.UserGroup)
                 .WithMany(g => g.Users)
                 .HasForeignKey(u => u.UserGroupId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // UserGroup
            b.Entity<UserGroup>(e =>
            {
                e.ToTable("UserGroups");
                e.HasKey(x => x.Id);
                e.Property(x => x.GroupName).HasMaxLength(100).IsRequired();
                e.HasIndex(x => x.GroupName).IsUnique();
            });

            // AccessRule
            b.Entity<AccessRule>(e =>
            {
                e.ToTable("AccessRules");
                e.HasKey(x => x.Id);
                e.Property(x => x.RuleName).HasMaxLength(100).IsRequired();

                e.HasOne(x => x.UserGroup)
                 .WithMany(g => g.AccessRules)
                 .HasForeignKey(x => x.UserGroupId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => new { x.UserGroupId, x.RuleName }).IsUnique();
            });
        }
    }
}


