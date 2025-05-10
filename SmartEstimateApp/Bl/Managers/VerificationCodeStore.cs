using Entities;
using System.Collections.Concurrent;

namespace Bl.Managers
{
    public class VerificationCodeStore
    {
        private class CodeEntry
        {
            public string Code { get; set; }
            public DateTime Expiry { get; set; }
            public VerificationPurpose Purpose { get; set; }
            public string SessionId { get; set; }
        }

        private static ConcurrentDictionary<string, CodeEntry> _codeStore = new ConcurrentDictionary<string, CodeEntry>();
        private static readonly TimeSpan _codeValidity = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Stores a verification code for the specified email and purpose
        /// </summary>
        /// <param name="email">The email address</param>
        /// <param name="code">The verification code</param>
        /// <param name="purpose">The purpose of verification</param>
        /// <returns>A unique session ID for this verification attempt</returns>
        public static string StoreCode(string email, string code, VerificationPurpose purpose)
        {
            string sessionId = Guid.NewGuid().ToString();

            var entry = new CodeEntry
            {
                Code = code,
                Expiry = DateTime.UtcNow.Add(_codeValidity),
                Purpose = purpose,
                SessionId = sessionId
            };

            // Create a compound key that includes both email and purpose
            string key = GetKey(email, purpose);
            _codeStore[key] = entry;

            return sessionId;
        }

        /// <summary>
        /// Verifies a code for the specified email and purpose
        /// </summary>
        /// <param name="email">The email address</param>
        /// <param name="code">The verification code to verify</param>
        /// <param name="purpose">The purpose of verification</param>
        /// <param name="sessionId">Optional session ID to ensure code is used by the same session</param>
        /// <returns>True if the code is valid, false otherwise</returns>
        public static bool VerifyCode(string email, string code, VerificationPurpose purpose, string sessionId = null)
        {
            string key = GetKey(email, purpose);

            if (_codeStore.TryGetValue(key, out var entry))
            {
                if (sessionId != null && entry.SessionId != sessionId)
                {
                    return false;
                }

                if (entry.Expiry > DateTime.UtcNow && entry.Code == code)
                {
                    _codeStore.TryRemove(key, out _);
                    return true;
                }

                if (entry.Expiry <= DateTime.UtcNow)
                {
                    _codeStore.TryRemove(key, out _);
                }
            }

            return false;
        }

        /// <summary>
        /// Invalidates a code for the specified email and purpose
        /// </summary>
        public static void InvalidateCode(string email, VerificationPurpose purpose)
        {
            string key = GetKey(email, purpose);
            _codeStore.TryRemove(key, out _);
        }

        /// <summary>
        /// Creates a unique key combining email and purpose
        /// </summary>
        private static string GetKey(string email, VerificationPurpose purpose)
        {
            return $"{email.ToLower().Trim()}:{purpose}";
        }

        /// <summary>
        /// Clears all expired codes
        /// </summary>
        public static void ClearExpiredCodes()
        {
            foreach (var entry in _codeStore)
            {
                if (entry.Value.Expiry <= DateTime.UtcNow)
                {
                    _codeStore.TryRemove(entry.Key, out _);
                }
            }
        }
    }
}