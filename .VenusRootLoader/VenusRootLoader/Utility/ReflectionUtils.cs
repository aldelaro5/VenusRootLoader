using System.Reflection;

namespace VenusRootLoader.Utility;

internal static class ReflectionUtils
{
    /// <summary>
    /// Shortcut for <see cref="BindingFlags"/> to simplify the use of reflections and make it work for any access level
    /// </summary>
    public const BindingFlags All = BindingFlags.Public
                                    | BindingFlags.NonPublic
                                    | BindingFlags.Instance
                                    | BindingFlags.Static
                                    | BindingFlags.GetField
                                    | BindingFlags.SetField
                                    | BindingFlags.GetProperty
                                    | BindingFlags.SetProperty;

    /// <summary>
    /// Shortcut for BindingFlags to simplify the use of reflections and make it work for any access level but only within the current type
    /// </summary>
    public const BindingFlags AllDeclared = All | BindingFlags.DeclaredOnly;

    extension(Type type)
    {
        public FieldInfo? FieldByName(string name) => type.GetField(name, All);
        public MethodInfo? MethodByName(string name) => type.GetMethod(name, All);

        public MethodInfo? MethodByName(string name, Type[] parameters) =>
            type.GetMethod(name, All, null, parameters, []);

        public ConstructorInfo? StaticConstructor(Type[] parameters) =>
            type.GetConstructor(All & ~BindingFlags.Instance, null, parameters, []);

        public MethodInfo PropertyGetterByName(string name) => type.GetProperty(name, All)!.GetGetMethod();
        public MethodInfo PropertySetterByName(string name) => type.GetProperty(name, All)!.GetSetMethod();
    }
}