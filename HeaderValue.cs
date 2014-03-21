using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace AE.Net.Mail {
	public struct HeaderValue {
		private string _RawValue;
		private SafeDictionary<string, string> _Values;

		public HeaderValue(string value)
			: this() {
			_Values = new SafeDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			_RawValue = (value ?? (value = string.Empty));
			_Values[string.Empty] = RawValue;

			var semicolon = value.IndexOf(';');
			if (semicolon > 0) {
				_Values[string.Empty] = value.Substring(0, semicolon).Trim();
				value = value.Substring(semicolon).Trim();
				ParseValues(_Values, value);
			}
		}
		public string Value { get { return this[string.Empty] ?? string.Empty; } }
		public string RawValue { get { return _RawValue ?? string.Empty; } }

		public string this[string name] {
			get { return _Values.Get(name, string.Empty); }
			set {
				_Values.Set(name, value);
			}
		}

		public static void ParseValues(IDictionary<string, string> result, string header) {
			while (header.Length > 0) {
				var eq = header.IndexOf('=');
				if (eq < 0) eq = header.Length;
				var name = header.Substring(0, eq).Trim().Trim(new[] { ';', ',' }).Trim();

				var value = header = header.Substring(Math.Min(header.Length, eq + 1)).Trim();

				if (value.StartsWith("\"")) {
					ProcessValue(1, ref header, ref value, '"');
				} else if (value.StartsWith("'")) {
					ProcessValue(1, ref header, ref value, '\'');
				} else {
					ProcessValue(0, ref header, ref value, ' ', ',', ';');
				}

				result.Set(name, value);
			}
		}

		private static void ProcessValue(int skip, ref string header, ref string value, params char[] lookFor) {
			var quote = value.IndexOfAny(lookFor, skip);
			if (quote < 0) quote = value.Length;
			header = header.Substring(Math.Min(quote + 1, header.Length));
			value = value.Substring(skip, quote - skip);
		}

		public override string ToString() {
			var props = _Values.Where(x => !string.IsNullOrEmpty(x.Key)).Select(x => x.Key + "=" + x.Value);
			return Value + (props.Any() ? ("; " + string.Join(", ", props)) : null);
		}
	}
}
