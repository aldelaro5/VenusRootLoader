using CommunityToolkit.Diagnostics;

namespace VenusRootLoader.Extensions;

internal static class EnumExtensions
{
    extension(Enum)
    {
        public static int GetNextIntEnumValue(Type enumType)
        {
            Guard.IsTrue(enumType.IsEnum);
            return Enum.GetValues(enumType).Cast<int>().Max() + 1;
        }

        public static T Parse<T>(string value)
            where T : Enum
        {
            return (T)Enum.Parse(typeof(T), value);
        }
    }
}