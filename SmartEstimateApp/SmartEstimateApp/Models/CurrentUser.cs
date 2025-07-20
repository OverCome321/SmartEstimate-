namespace SmartEstimateApp.Models
{
    public class CurrentUser
    {
        private static readonly Lazy<CurrentUser> _instance = new Lazy<CurrentUser>(() => new CurrentUser(), isThreadSafe: true);

        private User? _user;

        public static CurrentUser Instance => _instance.Value;

        public User? User
        {
            get => _user;
            private set => _user = value;
        }

        public bool IsAuthenticated => _user != null;

        public string Email => _user?.Email ?? string.Empty;

        public void SetUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            _user = user;
        }

        public void ClearUser()
        {
            _user = null;
        }
    }
}