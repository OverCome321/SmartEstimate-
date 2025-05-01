using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MigrationService.Data;

namespace MigrationService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting database migration for SmartEstimate...");

            try
            {
                var services = ConfigureServices();
                using var scope = services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SmartEstimateContext>();


                context.Database.Migrate();
                Console.WriteLine("Database migration completed successfully!");
                Console.WriteLine("You can now close this application.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Migration failed: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        private static IServiceProvider ConfigureServices()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();

            services.AddDbContext<SmartEstimateContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            return services.BuildServiceProvider();
        }
    }
}