using TSONify.Helpers;

namespace TSONify.Models;

internal abstract class ReadableType
{
    public static ReadableType Bool { get; } = new PredefinedReadableType("Boolean", reader => BinaryUtils.ReadBool(reader));

    public static ReadableType Int { get; } = new PredefinedReadableType("Int32", reader => BinaryUtils.ReadInt(reader));

    public static ReadableType Double { get; } = new PredefinedReadableType("Double", reader => BinaryUtils.ReadDouble(reader));

    public static ReadableType String { get; } = new PredefinedReadableType("String", reader =>
    {
        var header = BinaryUtils.ReadByte(reader);
        if (header == 0)
            return null;

        return BinaryUtils.ReadString(reader);
    });

    public static ReadableType Array(ReadableType? itemType, IReadOnlyList<ReadableType> customTypes) => new PredefinedArrayReadableType(customTypes, itemType);

    public static ReadableType Object(IReadOnlyList<ReadableType> customTypes) => new PredefinedReadableType("Object", reader =>
    {
        var header = BinaryUtils.ReadByte(reader);
        if (header == 0)
            return null;

        var dict = new Dictionary<string, object?>();
        var length = BinaryUtils.ReadUInt(reader);
        for (var i = 0; i < length; i++)
        {
            var key = BinaryUtils.ReadString(reader);
            var itemType = ResolveType(reader, customTypes) ?? throw new FormatException("Missing type");
            dict[key] = itemType.Read(reader);
        }

        return dict;
    });

    public static ReadableType ReadCustomType(BinaryReader reader)
    {
        return CustomReadableType.ReadType(reader);
    }

    public static void UpdateCustomTypes(IReadOnlyList<ReadableType> customTypes)
    {
        foreach (var type in customTypes.OfType<CustomReadableType>())
        {
            type.Update(customTypes);
        }
    }

    public abstract object? Read(BinaryReader reader);

    public static ReadableType? ResolveType(BinaryReader reader, IReadOnlyList<ReadableType> customTypes)
    {
        var typeId = reader.ReadUInt32();
        if (typeId == 0)
            return null;

        if (typeId == 1)
            return Bool;

        if (typeId == 2)
            return Int;

        if (typeId == 3)
            return Double;

        if (typeId == 4)
            return String;

        if (typeId == 5)
        {
            var itemType = ResolveType(reader, customTypes);
            return Array(itemType, customTypes);
        }

        if (typeId == 6)
            return Object(customTypes);

        return customTypes[(int)typeId - 7];
    }

    private class PredefinedReadableType : ReadableType
    {
        private readonly string _typeName;
        private readonly Func<BinaryReader, object?> _func;

        public PredefinedReadableType(string typeName, Func<BinaryReader, object?> func)
        {
            _typeName = typeName;
            _func = func;
        }

        public override object? Read(BinaryReader reader)
        {
            return _func(reader);
        }

        public override string ToString()
        {
            return _typeName;
        }
    }

    private class PredefinedArrayReadableType : ReadableType
    {
        private readonly IReadOnlyList<ReadableType> _customTypes;
        private readonly ReadableType? _itemType;

        public PredefinedArrayReadableType(IReadOnlyList<ReadableType> customTypes, ReadableType? itemType)
        {
            _customTypes = customTypes;
            _itemType = itemType;
        }

        public override object? Read(BinaryReader reader)
        {
            var header = BinaryUtils.ReadByte(reader);
            if (header == 0)
                return null;

            var length = BinaryUtils.ReadUInt(reader);
            var array = new object?[length];
            for (var i = 0; i < length; i++)
            {
                if (_itemType is not { } itemType)
                    itemType = ResolveType(reader, _customTypes) ?? throw new FormatException("Type missing");

                array[i] = itemType.Read(reader);
            }

            return array;
        }

        public override string ToString()
        {
            var typeName = _itemType?.ToString() ?? "unknown";
            return $"Array<{typeName}>";
        }
    }

    private class CustomReadableType : ReadableType
    {
        private IReadOnlyList<ReadableType>? _customTypes;

        public CustomReadableType(IReadOnlyList<ReadableProperty> properties)
        {
            Properties = properties;
        }

        public IReadOnlyList<ReadableProperty> Properties { get; }

        public override object? Read(BinaryReader reader)
        {
            if (_customTypes == null)
                throw new InvalidOperationException("Update() must be called before");

            var header = BinaryUtils.ReadByte(reader);
            if (header == 0)
                return null;

            var dict = new Dictionary<string, object?>();
            for (var i = 0; i < Properties.Count; i++)
            {
                var prop = Properties[i];
                dict[prop.Name] = prop.Read(reader, _customTypes!);
            }

            return dict;
        }

        public void Update(IReadOnlyList<ReadableType> customTypes)
        {
            _customTypes = customTypes;
            foreach (var prop in Properties)
            {
                prop.Update(customTypes);
            }
        }

        public override string ToString()
        {
            return $"({string.Join(", ", Properties.Select(p => p.ToString()))})";
        }

        public static ReadableType ReadType(BinaryReader reader)
        {
            var props = new List<ReadableProperty>();
            var typeIds = new List<uint>();
            while (true)
            {
                var propName = BinaryUtils.ReadString(reader);
                if (string.IsNullOrEmpty(propName))
                    break;

                typeIds.Clear();
                while (true)
                {
                    var typeId = BinaryUtils.ReadUInt(reader);
                    typeIds.Add(typeId);
                    if (typeId != 5)
                        break;
                }

                props.Add(new ReadableProperty(propName, typeIds.ToArray()));
            }

            return new CustomReadableType(props.ToArray());
        }
    }

    private class ReadableProperty
    {
        public ReadableProperty(string name, uint[] typeIds)
        {
            if (typeIds.Length == 0 || typeIds[^1] == 5)
                throw new ArgumentException();

            Name = name;
            TypeIds = typeIds;
        }

        public string Name { get; }

        public uint[] TypeIds { get; }

        public ReadableType? Type { get; private set; }

        public object? Read(BinaryReader reader, IReadOnlyList<ReadableType> customTypes)
        {
            if (Type is not { } propType)
                propType = ReadableType.ResolveType(reader, customTypes) ?? throw new FormatException("Missing type");

            return propType.Read(reader);
        }

        public void Update(IReadOnlyList<ReadableType> customTypes)
        {
            var type = (ReadableType?)null;
            for (var i = TypeIds.Length - 1; i >= 0; i--)
            {
                var typeId = TypeIds[i];
                type = typeId switch
                {
                    0 => null,
                    1 => ReadableType.Bool,
                    2 => ReadableType.Int,
                    3 => ReadableType.Double,
                    4 => ReadableType.String,
                    5 => ReadableType.Array(type, customTypes),
                    6 => ReadableType.Object(customTypes),
                    _ => customTypes[(int)typeId - 7]
                };
            }

            Type = type;
        }

        public override string ToString()
        {
            var typeName = Type?.ToString() ?? "unknown";
            return $"{typeName}: {Name}";
        }
    }
}
