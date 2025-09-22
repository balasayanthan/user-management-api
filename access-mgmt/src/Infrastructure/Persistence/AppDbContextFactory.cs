using System.IO;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence
{
    public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var slnRoot = Directory.GetCurrentDirectory();
            var apiSettings = Path.Combine(slnRoot, "src", "Api", "appsettings.json");

            var config = new ConfigurationBuilder()
                .SetBasePath(slnRoot)
                .AddJsonFile(apiSettings, optional: true)
                .Build();

            var cs = config.GetConnectionString("Default")
                     ?? "Server=DESKTOP-A0UCLME;Database=AccessMgmtDb_V1;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True";

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(cs)
                .Options;
            Console.WriteLine($"[EF] Using connection: {cs}");
            return new AppDbContext(options);
        }
    }
}
