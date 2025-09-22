using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.UserGroups.AnyAsync()) return;

            var admins = new UserGroup("Admins");
            var staff = new UserGroup("Staff");

            db.UserGroups.AddRange(admins, staff);
            await db.SaveChangesAsync();

            db.AccessRules.AddRange(
                new AccessRule(admins.Id, "CanManageUsers", true),
                new AccessRule(admins.Id, "CanViewReports", true),
                new AccessRule(staff.Id, "CanViewReports", true),
                new AccessRule(staff.Id, "CanManageUsers", false)
            );

            db.Users.AddRange(
                new User("sayan", "Admin", "sayan.admin@example.com", admins.Id),
                new User("bala", "Staff", "bala.staff@example.com", staff.Id, 42),
                new User("balasayanthan", "Logan", "balasayanthan.logan@example.com", staff.Id)
            );

            await db.SaveChangesAsync();
        }
    }
}
