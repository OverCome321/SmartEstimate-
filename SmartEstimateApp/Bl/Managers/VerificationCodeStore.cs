using System.Collections.Concurrent;

namespace Bl.Managers
{
    public class VerificationCodeStore
    {
        private class CodeEntry
        {
            public string Code { get; set; }
            public DateTime Expiry { get; set; }
        }

        private static ConcurrentDictionary<string, CodeEntry> _codeStore = new ConcurrentDictionary<string, CodeEntry>();
        private static readonly TimeSpan _codeValidity = TimeSpan.FromMinutes(5);

        public static void StoreCode(string email, string code)
        {
            var entry = new CodeEntry
            {
                Code = code,
                Expiry = DateTime.UtcNow.Add(_codeValidity)
            };
            _codeStore[email] = entry;
        }

        public static bool VerifyCode(string email, string code)
        {
            if (_codeStore.TryGetValue(email, out var entry))
            {
                if (entry.Expiry > DateTime.UtcNow && entry.Code == code)
                {
                    _codeStore.TryRemove(email, out _);
                    return true;
                }
                _codeStore.TryRemove(email, out _);
            }
            return false;
        }

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
