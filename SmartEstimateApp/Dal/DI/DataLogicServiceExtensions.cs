using Dal.DbModels;
using Dal.Interfaces;
using Dal.Layers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dal.DI
{
    public static class DataAccessServiceExtensions
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = GetConnectionString(configuration);
            services.AddDbContext<SmartEstimateDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddAutoMapper(typeof(UserDal).Assembly);

            services.AddScoped<IUserDal, UserDal>();
            services.AddScoped<IClientDal, ClientDal>();
            services.AddScoped<IProjectDal, ProjectDal>();

            return services;
        }

        private static string GetConnectionString(IConfiguration configuration)
        {
            string machineName = Environment.MachineName;
            switch (machineName)
            {
                case "DESKTOP-K81FSPL":
                    return configuration.GetConnectionString("Connection1");
                case "DESKTOP-RE0M47N":
                    return configuration.GetConnectionString("Connection2");
                default:
                    return configuration.GetConnectionString("Connection1");
            }
        }
    }
}