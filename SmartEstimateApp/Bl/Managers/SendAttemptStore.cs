using System.Collections.Concurrent;

namespace Bl.Managers
{
    /// <summary>
    /// Управляет лимитом отправки кодов верификации для предотвращения спама
    /// </summary>
    public class SendAttemptStore
    {
        private class AttemptEntry
        {
            public int AttemptCount { get; set; }
            public DateTime? CooldownUntil { get; set; }
        }

        private static readonly ConcurrentDictionary<string, AttemptEntry> _attemptStore = new ConcurrentDictionary<string, AttemptEntry>();
        private static readonly TimeSpan _cooldownPeriod = TimeSpan.FromMinutes(1);
        private static readonly int _maxAttempts = 1;

        /// <summary>
        /// Проверяет, можно ли отправить код на указанный email
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <returns>Статус и сообщение об ошибке (если есть)</returns>
        public static (bool CanSend, string ErrorMessage, int? RemainingSeconds) CanSendCode(string email)
        {
            var entry = _attemptStore.GetOrAdd(email, _ => new AttemptEntry());

            // Проверка периода ожидания
            if (entry.CooldownUntil.HasValue && DateTime.UtcNow < entry.CooldownUntil.Value)
            {
                var remainingSeconds = (int)Math.Ceiling((entry.CooldownUntil.Value - DateTime.UtcNow).TotalSeconds);
                return (false, $"Слишком много попыток. Подождите {remainingSeconds} секунд перед следующей отправкой.", remainingSeconds);
            }

            // Сброс счетчика, если период ожидания истек
            if (entry.CooldownUntil.HasValue && DateTime.UtcNow >= entry.CooldownUntil.Value)
            {
                entry.AttemptCount = 0;
                entry.CooldownUntil = null;
            }

            return (true, null, null);
        }

        /// <summary>
        /// Записывает попытку отправки и устанавливает период ожидания при достижении лимита
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <returns>Информация о следующей возможной отправке</returns>
        public static (bool IsInCooldown, int? CooldownSeconds) RecordAttempt(string email)
        {
            var entry = _attemptStore.GetOrAdd(email, _ => new AttemptEntry());
            entry.AttemptCount++;

            if (entry.AttemptCount >= _maxAttempts)
            {
                entry.CooldownUntil = DateTime.UtcNow.Add(_cooldownPeriod);
                return (true, (int)_cooldownPeriod.TotalSeconds);
            }

            return (false, null);
        }

        /// <summary>
        /// Удаляет просроченные записи из хранилища
        /// </summary>
        //public static void ClearExpiredAttempts()
        //{
        //    foreach (var pair in _attemptStore)
        //    {
        //        if (pair.Value.CooldownUntil.HasValue && DateTime.UtcNow >= pair.Value.CooldownUntil.Value)
        //        {
        //            _attemptStore.TryRemove(pair.Key, out _);
        //        }
        //    }
        //}
    }
}