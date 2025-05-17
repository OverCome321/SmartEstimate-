using Common.Managers;
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
            string connectionString = ConnectionStringManager.GetConnectionString(configuration);
            services.AddDbContext<SmartEstimateDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddAutoMapper(typeof(UserDal).Assembly);

            services.AddScoped<IUserDal, UserDal>();
            services.AddScoped<IClientDal, ClientDal>();
            services.AddScoped<IProjectDal, ProjectDal>();

            return services;
        }
    }
}