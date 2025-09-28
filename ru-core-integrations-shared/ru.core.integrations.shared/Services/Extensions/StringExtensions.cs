using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ru.core.integrations.shared.Services.Extensions
{
    public static class StringExtensions
    {
        public static bool EqualsIgnoreCase(this string? value, string? compareWith)
        {
            // check to handle nulls and reference equality
            if (value == compareWith)
                return true;

            return string.Equals(value, compareWith, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsNullOrWhitespace(this string? value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static string? LimitToMaxLength(this string? value, int maxLength)
        {
            if (value == null || value.Length <= maxLength)
                return value;

            return value[..maxLength];
        }
    }
}
