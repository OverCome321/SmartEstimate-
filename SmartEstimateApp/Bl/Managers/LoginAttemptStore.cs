using System.Collections.Concurrent;

namespace Bl.Managers
{
    public static class LoginAttemptStore
    {
        private static readonly ConcurrentDictionary<string, (int AttemptCount, DateTime? LockoutEnd)> _attempts = new();
        private const int MaxAttempts = 5;
        private const int LockoutSeconds = 60;

        public static (bool CanLogin, string ErrorMessage, int? RemainingSeconds) CanLogin(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email не указан", null);

            var now = DateTime.UtcNow;
            if (_attempts.TryGetValue(email, out var data))
            {
                if (data.LockoutEnd.HasValue && now < data.LockoutEnd.Value)
                {
                    var remainingSeconds = (int)Math.Ceiling((data.LockoutEnd.Value - now).TotalSeconds);
                    return (false, $"Слишком много попыток. Попробуйте снова через {remainingSeconds} сек.", remainingSeconds);
                }
            }

            return (true, null, null);
        }

        public static (bool IsLockedOut, int? CooldownSeconds) RecordFailedAttempt(string email)
        {
            var now = DateTime.UtcNow;
            var data = _attempts.AddOrUpdate(
                email,
                (1, null),
                (key, old) =>
                {
                    if (old.LockoutEnd.HasValue && now < old.LockoutEnd.Value)
                        return old;

                    int newCount = old.AttemptCount + 1;
                    DateTime? lockoutEnd = newCount >= MaxAttempts ? now.AddSeconds(LockoutSeconds) : null;
                    return (newCount, lockoutEnd);
                });

            if (data.LockoutEnd.HasValue && now < data.LockoutEnd.Value)
            {
                var cooldownSeconds = (int)Math.Ceiling((data.LockoutEnd.Value - now).TotalSeconds);
                return (true, cooldownSeconds);
            }

            return (false, null);
        }

        public static void ResetAttempts(string email)
        {
            _attempts.TryRemove(email, out _);
        }
    }
}
