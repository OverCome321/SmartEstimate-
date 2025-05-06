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
            services.AddDbContext<SmartEstimateDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddAutoMapper(typeof(UserDal).Assembly);

            services.AddScoped<IUserDal, UserDal>();

            services.AddScoped<IClientDal, ClientDal>();

            services.AddScoped<IProjectDal, ProjectDal>();

            return services;
        }
    }
}
