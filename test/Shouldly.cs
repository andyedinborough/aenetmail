
using System;
using System.Collections.Generic;
namespace Shouldly {
	internal static class exts {

		public static void ShouldBe(this object input) {
			input.ShouldNotBe(null);
		}

		public static void ShouldNotBe(this object input) {
			input.ShouldBe(null);
		}

		public static bool ShouldNotBe(this bool input) {
			input.ShouldNotBe(false);
			return input;
		}

		public static bool ShouldBe(this bool input) {
			input.ShouldBe(true);
			return input;
		}

		public static void ShouldBeInRange<T>(this T input, T a, T b) where T : IConvertible {
			input.ShouldBeGreaterThanOrEqualTo(a);
			input.ShouldBeLessThanOrEqualTo(b);
		}

		public static void ShouldBeLessThanOrEqualTo<T>(this T input, T b) where T : IConvertible {
			if (input.Equals(b)) return;
			input.ShouldBeLessThan(b);
		}

		public static void ShouldNotBeNullOrEmpty<T>(this IEnumerable<T> input) {
			input.ShouldNotBe(null);
			input.ShouldNotBeEmpty();
		}

		public static void ShouldBeNullOrEmpty<T>(this IEnumerable<T> list) {
			if (list == null) return;
			list.ShouldBeEmpty();
		}
	}
}
