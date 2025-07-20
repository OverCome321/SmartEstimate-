namespace Bl.DI
{
    /// <summary>
    /// Параметры конфигурации для слоя бизнес-логики
    /// </summary>
    public class BusinessLogicOptions
    {
        /// <summary>
        /// Включить расширенную валидацию сущностей
        /// </summary>
        public bool EnableExtendedValidation { get; set; } = false;

        /// <summary>
        /// Минимальная длина пароля пользователя
        /// </summary>
        public int MinPasswordLength { get; set; } = 8;

        /// <summary>
        /// Требовать сложный пароль (буквы, цифры, спецсимволы)
        /// </summary>
        public bool RequireComplexPassword { get; set; } = true;

        /// <summary>
        /// Ограничить число неудачных попыток входа
        /// </summary>
        public int MaxFailedLoginAttempts { get; set; } = 5;
    }
}