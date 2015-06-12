
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Shouldly {
	internal static class exts {

        #region Methods

        public static void ShouldBe<T>(this T input, T value)
        {
            Assert.Equal(value, input);
        }
        public static void ShouldBe(this object input)
        {
            input.ShouldNotBe(null);
        }

        public static bool ShouldBe(this bool input)
        {
            input.ShouldBe(true);
            return input;
        }

        public static void ShouldBeEmpty<T>(this IEnumerable<T> list)
        {
            Assert.False(list.Any());
        }

        public static void ShouldBeGreaterThan<T>(this T input, T other) where T : IConvertible
        {
            var format = System.Globalization.CultureInfo.CurrentCulture;
            Assert.True(input.ToDouble(format) > other.ToDouble(format));
        }

        public static void ShouldBeGreaterThanOrEqualTo<T>(this T input, T other) where T : IConvertible
        {
            var format = System.Globalization.CultureInfo.CurrentCulture;
            Assert.True(input.ToDouble(format) > other.ToDouble(format) || input.Equals(other));
        }

        public static void ShouldBeInRange<T>(this T input, T a, T b) where T : IConvertible
        {
            input.ShouldBeGreaterThanOrEqualTo(a);
            input.ShouldBeLessThanOrEqualTo(b);
        }

        public static void ShouldBeLessThan<T>(this T input, T other) where T : IConvertible
        {
            var format = System.Globalization.CultureInfo.CurrentCulture;
            Assert.True(input.ToDouble(format) < other.ToDouble(format));
        }

        public static void ShouldBeLessThanOrEqualTo<T>(this T input, T b) where T : IConvertible
        {
            if (input.Equals(b)) return;
            input.ShouldBeLessThan(b);
        }

        public static void ShouldBeNullOrEmpty<T>(this IEnumerable<T> list)
        {
            if (list == null) return;
            list.ShouldBeEmpty();
        }

        public static void ShouldContain(this string input, string other)
        {
            Assert.True(input?.Contains(other));
        }

        public static void ShouldNotBe<T>(this T input, T value)
        {
            Assert.NotEqual(input, value);
        }
		public static void ShouldNotBe(this object input) {
			input.ShouldBe(null);
		}

		public static bool ShouldNotBe(this bool input) {
			input.ShouldNotBe(false);
			return input;
		}
        public static void ShouldNotBeEmpty<T>(this IEnumerable<T> list)
        {
            Assert.True(list.Any());
        }

        public static void ShouldNotBeNullOrEmpty<T>(this IEnumerable<T> input)
        {
            input.ShouldNotBe(null);
            input.ShouldNotBeEmpty();
        }

        public static void ShouldStartWith(this string input, string other)
        {
            Assert.True(input?.StartsWith(other));
        }

        #endregion
    }
}
