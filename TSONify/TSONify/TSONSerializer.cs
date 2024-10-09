using System.Collections;
using TSONify.Helpers;

namespace TSONify;

public class TSONSerializer
{
    public byte[] Serialize(object obj, Type type)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        BinaryUtils.Write(writer, (ushort)0x54FA);
        BinaryUtils.Write(writer, (byte)1);

        var types = GetTypes(type);
        var typesBuffer = SerializeTypes(types);
        var contentBuffer = SerializeContent(types, obj, type);

        BinaryUtils.Write(writer, (uint)typesBuffer.Length);
        BinaryUtils.Write(writer, (uint)contentBuffer.Length);

        writer.Write(typesBuffer);
        writer.Write(contentBuffer);

        return stream.ToArray();
    }

    private byte[] SerializeTypes(IReadOnlyList<Models.TypeInfo> types)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        var customTypes = types.Select(t => t.Type).ToList();
        BinaryUtils.Write(writer, (uint)types.Count);
        foreach (var type in types)
        {
            foreach (var property in type.Properties)
            {
                BinaryUtils.Write(writer, property.Name);
                WriteTypeId(writer, property.Type, customTypes);
            }

            BinaryUtils.Write(writer, (byte)0);
        }

        return stream.ToArray();
    }

    private byte[] SerializeContent(IReadOnlyList<Models.TypeInfo> types, object? obj, Type type)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        SerializeContent(writer, types, obj, type);
        return stream.ToArray();
    }

    private static void SerializeContent(BinaryWriter writer, IReadOnlyList<Models.TypeInfo> types, object? obj, Type type)
    {
        if (obj == null)
        {
            BinaryUtils.Write(writer, (byte)0);
            return;
        }

        var typeId = GetPredefinedTypeId(type);
        if (typeId >= 0)
        {
            switch (typeId)
            {
                case 1:
                    BinaryUtils.Write(writer, Convert.ToBoolean(obj));
                    break;
                case 2:
                    BinaryUtils.Write(writer, Convert.ToInt32(obj));
                    break;
                case 3:
                    BinaryUtils.Write(writer, Convert.ToDouble(obj));
                    break;
                case 4:
                    BinaryUtils.Write(writer, obj.ToString()!);
                    break;
                case 5:
                {
                    if (type.IsArray)
                    {
                        var arr = (Array)obj;
                        var length = arr.Length;
                        var itemType = type.GetElementType()!;
                        BinaryUtils.Write(writer, (uint)length);
                        for (var i = 0; i < length; i++)
                        {
                            SerializeContent(writer, types, arr.GetValue(i), itemType);
                        }
                    }
                    else
                    {
                        var itemType = type.GetGenericArguments()[0];
                        var listType = typeof(IReadOnlyCollection<>).MakeGenericType(itemType);
                        var length = (int)listType.GetProperty("Count")!.GetValue(obj)!;
                        var index = 0;
                        foreach (var item in (IEnumerable)obj)
                        {
                            SerializeContent(writer, types, item, listType);
                        }

                        if (index != length)
                            throw new InvalidOperationException("Items count is not equals to specified length");
                    }

                    break;
                }
            }
        }
        else
        {
            var info = types.FirstOrDefault(t => t.Type == type);
            if (info == null)
                throw new InvalidOperationException($"Cannot find custom type '{type}'");

            foreach (var prop in info.Properties)
            {
                SerializeContent(writer, types, prop.GetValue(obj), prop.Type);
            }
        }
    }

    private static void WriteTypeId(BinaryWriter writer, Type type, List<Type> customTypes)
    {
        var typeId = GetPredefinedTypeId(type);
        if (typeId >= 0)
        {
            BinaryUtils.Write(writer, (uint)typeId);
            if (typeId == 5)
                WriteTypeId(writer, type.IsArray ? type.GetElementType()! : type.GetGenericArguments()[0], customTypes);
        }
        else
        {
            BinaryUtils.Write(writer, (uint)(customTypes.IndexOf(type) + 7));
        }
    }

    private static IReadOnlyList<Models.TypeInfo> GetTypes(Type type)
    {
        var hashSet = new HashSet<Type>();
        return AddTypes(type, hashSet).ToArray();
    }

    private static IEnumerable<Models.TypeInfo> AddTypes(Type type, HashSet<Type> hashSet)
    {
        var predefinedId = GetPredefinedTypeId(type);
        if (predefinedId == 5)
        {
            type = type.IsArray ? type.GetElementType()! : type.GetGenericArguments()[0];
            foreach (var p in AddTypes(type, hashSet))
            {
                yield return p;
            }

            yield break;
        }

        if (predefinedId >= 0 || !hashSet.Add(type))
        {
            yield break;
        }

        var info = new Models.TypeInfo(type);
        foreach (var prop in info.Properties)
        {
            foreach (var p in AddTypes(prop.Type, hashSet))
            {
                yield return p;
            }
        }

        yield return info;
    }

    private static int GetPredefinedTypeId(Type type)
    {
        if (type == typeof(bool))
            return 1;

        if (type == typeof(sbyte) ||
            type == typeof(byte) ||
            type == typeof(short) ||
            type == typeof(ushort) ||
            type == typeof(int) ||
            type == typeof(uint) ||
            type == typeof(long) ||
            type == typeof(ulong))
        {
            return 2;
        }

        if (type == typeof(float) || type == typeof(double))
            return 3;

        if (type == typeof(char) || type == typeof(string))
            return 4;

        if (type.IsArray && type.GetArrayRank() == 1)
            return 5;

        if (type.IsConstructedGenericType)
        {
            var genericTypes = type.GetGenericArguments();
            if (genericTypes.Length == 1)
            {
                var listType = typeof(List<>).MakeGenericType(genericTypes);
                if (type.IsAssignableFrom(listType))
                {
                    return 5;
                }
            }
            else if (genericTypes.Length == 2 && genericTypes[0] == typeof(string))
            {
                var dictType = typeof(Dictionary<,>).MakeGenericType(genericTypes);
                if (type.IsAssignableFrom(dictType))
                {
                    return 6;
                }
            }
        }

        return -1;
    }
}
