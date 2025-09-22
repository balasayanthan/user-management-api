using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions
{
    public interface IAppDbContext
    {
        DbSet<Person> People { get; }
        DbSet<Admin> Admins { get; }
        DbSet<User> Users { get; }
        DbSet<UserGroup> UserGroups { get; }
        DbSet<AccessRule> AccessRules { get; }
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
