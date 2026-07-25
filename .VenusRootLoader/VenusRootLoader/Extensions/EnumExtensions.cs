using CommunityToolkit.Diagnostics;

namespace VenusRootLoader.Extensions;

/// <summary>
/// Extensions to <see cref="Enum"/> types.
/// </summary>
internal static class EnumExtensions
{
    extension(Enum)
    {
        /// <summary>
        /// Gets the next free int value from an enum assuming all of its underlying values are sequential.
        /// </summary>
        /// <param name="enumType">The type of the enum.</param>
        /// <returns>The first int value of the enum which has no backing enum value.</returns>
        public static int GetNextIntEnumValue(Type enumType)
        {
            Guard.IsTrue(enumType.IsEnum);
            return Enum.GetValues(enumType).Cast<int>().Max() + 1;
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants
        /// specified by <typeparamref name="TEnum"/> to an equivalent enumerated object.
        /// </summary>
        /// <param name="value">A string containing the name or value to convert.</param>
        /// <typeparam name="TEnum">An enumeration type.</typeparam>
        /// <returns>An object of type <typeparamref name="TEnum"/> whose value is represented by value.</returns>
        public static TEnum Parse<TEnum>(string value)
            where TEnum : Enum
        {
            return (TEnum)Enum.Parse(typeof(TEnum), value);
        }
    }
}