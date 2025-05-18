using Bl.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Bl.DI
{
    /// <summary>
    /// Содержит методы расширения для регистрации сервисов бизнес-логики в DI-контейнере
    /// </summary>
    public static class BusinessLogicServiceExtensions
    {
        /// <summary>
        /// Регистрирует сервисы бизнес-логики в контейнере зависимостей
        /// </summary>
        /// <param name="services">Коллекция сервисов</param>
        /// <returns>Коллекция сервисов с зарегистрированными компонентами бизнес-логики</returns>
        public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
        {
            services.AddScoped<IUserBL, UserBL>();
            services.AddScoped<IClientBL, ClientBL>();
            services.AddScoped<IProjectBL, ProjectBL>();
            services.AddScoped<IEmailService, EmailServiceBL>();
            services.AddScoped<EmailVerificationServiceBL>();
            return services;
        }
        /// <summary>
        /// Регистрирует сервисы бизнес-логики в контейнере зависимостей
        /// с дополнительными настройками через опции
        /// </summary>
        /// <param name="services">Коллекция сервисов</param>
        /// <param name="configureOptions">Делегат для настройки опций бизнес-логики</param>
        /// <returns>Коллекция сервисов с зарегистрированными компонентами бизнес-логики</returns>
        public static IServiceCollection AddBusinessLogic(
            this IServiceCollection services,
            Action<BusinessLogicOptions> configureOptions)
        {
            services.AddBusinessLogic();

            services.Configure(configureOptions);
            return services;
        }
    }
}