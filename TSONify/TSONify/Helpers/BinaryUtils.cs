using System.Buffers.Binary;
using System.Text;

namespace TSONify.Helpers;

internal static class BinaryUtils
{
    public static bool ReadBool(BinaryReader reader)
    {
        return reader.ReadByte() != 0;
    }

    public static byte ReadByte(BinaryReader reader)
    {
        return reader.ReadByte();
    }

    public static short ReadShort(BinaryReader reader)
    {
        return (short)ReadUShort(reader);
    }

    public static ushort ReadUShort(BinaryReader reader)
    {
        return (ushort)(reader.ReadByte() | (uint)reader.ReadByte() << 8);
    }

    public static int ReadInt(BinaryReader reader)
    {
        return (int)ReadUInt(reader);
    }

    public static uint ReadUInt(BinaryReader reader)
    {
        return reader.ReadByte() |
               (uint)reader.ReadByte() << 8 |
               (uint)reader.ReadByte() << 16 |
               (uint)reader.ReadByte() << 24;
    }

    public static long ReadLong(BinaryReader reader)
    {
        return (long)ReadULong(reader);
    }

    public static ulong ReadULong(BinaryReader reader)
    {
        return reader.ReadByte() |
               (ulong)reader.ReadByte() << 8 |
               (ulong)reader.ReadByte() << 16 |
               (ulong)reader.ReadByte() << 24 |
               (ulong)reader.ReadByte() << 32 |
               (ulong)reader.ReadByte() << 40 |
               (ulong)reader.ReadByte() << 48 |
               (ulong)reader.ReadByte() << 56;
    }

    public static float ReadFloat(BinaryReader reader)
    {
        var tmp = ReadUInt(reader);
        if (!BitConverter.IsLittleEndian)
            tmp = BinaryPrimitives.ReverseEndianness(tmp);

        return BitConverter.UInt32BitsToSingle(tmp);
    }

    public static double ReadDouble(BinaryReader reader)
    {
        var tmp = ReadULong(reader);
        if (!BitConverter.IsLittleEndian)
            tmp = BinaryPrimitives.ReverseEndianness(tmp);

        return BitConverter.UInt64BitsToDouble(tmp);
    }

    public static string ReadString(BinaryReader reader)
    {
        var list = new List<byte>();
        while (reader.ReadByte() is not 0 and var c)
        {
            list.Add(c);
        }

        return Encoding.UTF8.GetString(list.ToArray());
    }

    public static void WriteBool(BinaryWriter writer, bool value)
    {
        writer.Write(value ? (byte)1 : (byte)0);
    }

    public static void WriteByte(BinaryWriter writer, byte value)
    {
        writer.Write(value);
    }

    public static void WriteShort(BinaryWriter writer, short value)
    {
        WriteUShort(writer, (ushort)value);
    }

    public static void WriteUShort(BinaryWriter writer, ushort value)
    {
        writer.Write((byte)(value & 0xFF));
        writer.Write((byte)((value >> 8) & 0xFF));
    }

    public static void WriteInt(BinaryWriter writer, int value)
    {
        WriteUInt(writer, (uint)value);
    }

    public static void WriteUInt(BinaryWriter writer, uint value)
    {
        writer.Write((byte)(value & 0xFF));
        writer.Write((byte)((value >> 8) & 0xFF));
        writer.Write((byte)((value >> 16) & 0xFF));
        writer.Write((byte)((value >> 24) & 0xFF));
    }

    public static void WriteLong(BinaryWriter writer, long value)
    {
        WriteULong(writer, (ulong)value);
    }

    public static void WriteULong(BinaryWriter writer, ulong value)
    {
        writer.Write((byte)(value & 0xFF));
        writer.Write((byte)((value >> 8) & 0xFF));
        writer.Write((byte)((value >> 16) & 0xFF));
        writer.Write((byte)((value >> 24) & 0xFF));
        writer.Write((byte)((value >> 32) & 0xFF));
        writer.Write((byte)((value >> 40) & 0xFF));
        writer.Write((byte)((value >> 48) & 0xFF));
        writer.Write((byte)((value >> 56) & 0xFF));
    }

    public static void WriteFloat(BinaryWriter writer, float value)
    {
        var tmp = BitConverter.SingleToUInt32Bits(value);
        if (!BitConverter.IsLittleEndian)
            tmp = BinaryPrimitives.ReverseEndianness(tmp);

        WriteUInt(writer, tmp);
    }

    public static void WriteDouble(BinaryWriter writer, double value)
    {
        var tmp = BitConverter.DoubleToInt64Bits(value);
        if (!BitConverter.IsLittleEndian)
            tmp = BinaryPrimitives.ReverseEndianness(tmp);

        WriteLong(writer, tmp);
    }

    public static void WriteString(BinaryWriter writer, string value)
    {
        var content = Encoding.UTF8.GetBytes(value);
        writer.Write(content);
        writer.Write((byte)0);
    }
}
