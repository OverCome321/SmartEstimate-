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
    }
}