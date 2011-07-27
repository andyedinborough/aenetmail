using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace AE.Net.Mail {
    internal static class Utilities {
        private static Regex rxStripHtmlComment = new Regex("\\<\\!\\-\\-(.*?)\\-\\-\\>", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex rxStripHtmlHEAD = new Regex("\\<head[^>]*\\>(.*?)\\</head\\>", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex rxStripHtmlSCRIPT = new Regex("\\<script[^>]*\\>(.*?)\\</script\\>", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex rxStripHtmlSTYLE = new Regex("\\<style[^>]*\\>(.*?)\\</style\\>", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex rxStripHtml = new Regex("\\<[^>]+\\>", RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex rxStripHtmlSpacing = new Regex("\\s+", RegexOptions.Singleline | RegexOptions.Compiled);

        public static string StripHtml(this string html, params string[] tagsToStripEntirely) {
            if (html == null) return string.Empty;
            if (tagsToStripEntirely != null) {
                foreach (var tag in tagsToStripEntirely) {
                    html = Regex.Replace(html, "\\<" + tag + "[^>]*\\>(.*?)\\</" + tag + "\\>", " ", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                }
            }

            html = rxStripHtmlComment.Replace(html, " ");
            html = rxStripHtmlHEAD.Replace(html, " ");
            html = rxStripHtmlSCRIPT.Replace(html, " ");
            html = rxStripHtmlSTYLE.Replace(html, " ");
            html = rxStripHtml.Replace(html, " ");
            html = rxStripHtmlSpacing.Replace(html, " ");
            html = html.Trim();
            html = System.Net.WebUtility.HtmlDecode(html);
            return html;
        }

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

        private static Regex rxHtml = new Regex(@"\<(b|i|p|em|a|span|table|div|html|body)[^>]*\>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static bool LooksLikeHtml(this string html) {
            return rxHtml.IsMatch(html);
        }
    }
}
