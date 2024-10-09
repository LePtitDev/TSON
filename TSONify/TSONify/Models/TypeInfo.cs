using System.Reflection;
using System.Text.Json.Serialization;

namespace TSONify.Models;

internal class TypeInfo
{
    public TypeInfo(Type type)
    {
        Type = type;
        Properties = GetTypeProperties(type).ToArray();
    }

    public Type Type { get; }

    public IReadOnlyList<PropertyInfo> Properties { get; }

    public override string ToString()
    {
        return $"{Type.Name} ({Properties.Count} properties)";
    }

    private static IEnumerable<PropertyInfo> GetTypeProperties(Type type)
    {
        foreach (var property in type.GetProperties())
        {
            if (!property.CanRead)
                continue;

            yield return new PropertyInfo(property);
        }
    }
}

internal class PropertyInfo
{
    private readonly Func<object, object?> _getter;

    public PropertyInfo(System.Reflection.PropertyInfo property)
    {
        _getter = property.GetValue;
        Name = ResolveName(property);
        Type = property.PropertyType;
    }

    public string Name { get; }

    public Type Type { get; }

    public object? GetValue(object instance)
    {
        return _getter.Invoke(instance);
    }

    public override string ToString()
    {
        return $"{Name} ({Type})";
    }

    private static string ResolveName(System.Reflection.PropertyInfo property)
    {
        var attributes = property.GetCustomAttributes(typeof(JsonPropertyNameAttribute), true);
        return attributes.Length != 0 ? ((JsonPropertyNameAttribute)attributes[0]).Name : property.Name;
    }
}
