namespace Entities
{
    public static class ErrorMessages
    {
        #region User
        public const string RoleNotSpecified = "Роль пользователя не найдена в базе данных.";
        public const string EmailAlreadyExists = "Пользователь с таким email уже существует.";
        public const string PasswordEmpty = "Пароль не может быть пустым.";
        public const string EmailEmpty = "Email не может быть пустым.";
        public const string EmailInvalidFormat = "Email имеет неверный формат.";
        public const string PasswordTooShort = "Пароль должен содержать не менее {0} символов";
        public const string ComplexPasswordRequired = "Пароль должен содержать буквы, цифры и специальные символы";
        #endregion

        #region Client
        public const string UserIdRequired = "Идентификатор пользователя обязателен для поиска клиентов.";
        public const string ClientEntityNull = "Сущность клиента не может быть null.";
        public const string InvalidEmailFormat = "Неверный формат email.";
        public const string InvalidPhoneFormat = "Неверный формат телефона.";
        public const string UserIdNotSpecified = "Идентификатор пользователя должен быть указан.";
        public const string ClientEmailAlreadyExists = "Клиент с таким email уже существует для указанного пользователя.";
        public const string ClientPhoneAlreadyExists = "Клиент с таким номером телефона уже существует для указанного пользователя.";
        #endregion

        #region Projects
        public const string ProjectEntityNull = "Сущность проекта не может быть пустой";
        public const string ProjectNameRequired = "Название проекта обязательно";
        public const string ClientIdNotSpecified = "Идентификатор клиента не указан";
        public const string ProjectNameAlreadyExists = "Проект с таким названием уже существует у этого клиента";
        #endregion
    }
}