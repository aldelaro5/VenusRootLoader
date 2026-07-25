using System.Globalization;
using System.Text;

namespace VenusRootLoader.Extensions;

internal static class StringBuilderExtensions
{
    extension(StringBuilder sb)
    {
        public StringBuilder AppendInvariant(bool value) =>
            sb.Append(value.ToString(CultureInfo.InvariantCulture));

        public StringBuilder AppendInvariant(int value) =>
            sb.Append(value.ToString(CultureInfo.InvariantCulture));

        public StringBuilder AppendInvariant(float value) =>
            sb.Append(value.ToString(CultureInfo.InvariantCulture));

        public StringBuilder AppendInvariant(string value) =>
            sb.Append(value.ToString(CultureInfo.InvariantCulture));
    }
}