using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace AE.Net.Mail {
    internal static class Utilities {
        public static VT Get<KT, VT>(this IDictionary<KT, VT> dictionary, KT key, VT defaultValue = default(VT)) {
            if (dictionary == null) return defaultValue;
            VT value;
            if (dictionary.TryGetValue(key, out value))
                return value;
            return defaultValue;
        }

        public static void Set<KT, VT>(this IDictionary<KT, VT> dictionary, KT key, VT value) {
            if (!dictionary.ContainsKey(key))
                lock (dictionary)
                    if (!dictionary.ContainsKey(key)) {
                        dictionary.Add(key, value);
                        return;
                    }

            dictionary[key] = value;
        }

        public static MailAddress ToEmailAddress(this string input) {
            try {
                return new MailAddress(input);
            } catch (Exception) {
                return null;
            }
        }

        public static bool Is(this string input, string other) {
            return string.Equals(input, other, StringComparison.OrdinalIgnoreCase);
        }
    }
}
