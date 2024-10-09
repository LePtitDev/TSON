using System;
using System.Buffers.Binary;
using System.Text;

namespace TSONify.Helpers;

internal static class BinaryUtils
{
    public static void Write(BinaryWriter writer, bool value)
    {
        writer.Write(value ? (byte)1 : (byte)0);
    }

    public static void Write(BinaryWriter writer, short value)
    {
        Write(writer, (ushort)value);
    }

    public static void Write(BinaryWriter writer, ushort value)
    {
        writer.Write((byte)(value & 0xFF));
        writer.Write((byte)((value >> 8) & 0xFF));
    }

    public static void Write(BinaryWriter writer, int value)
    {
        Write(writer, (uint)value);
    }

    public static void Write(BinaryWriter writer, uint value)
    {
        writer.Write((byte)(value & 0xFF));
        writer.Write((byte)((value >> 8) & 0xFF));
        writer.Write((byte)((value >> 16) & 0xFF));
        writer.Write((byte)((value >> 24) & 0xFF));
    }

    public static void Write(BinaryWriter writer, long value)
    {
        Write(writer, (ulong)value);
    }

    public static void Write(BinaryWriter writer, ulong value)
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

    public static void Write(BinaryWriter writer, double value)
    {
        var tmp = BitConverter.DoubleToInt64Bits(value);
        if (!BitConverter.IsLittleEndian)
            tmp = BinaryPrimitives.ReverseEndianness(tmp);

        Write(writer, tmp);
    }

    public static void Write(BinaryWriter writer, float value)
    {
        var tmp = BitConverter.SingleToUInt32Bits(value);
        if (!BitConverter.IsLittleEndian)
            tmp = BinaryPrimitives.ReverseEndianness(tmp);

        Write(writer, tmp);
    }

    public static void Write(BinaryWriter writer, string value)
    {
        var content = Encoding.UTF8.GetBytes(value);
        writer.Write(content);
        writer.Write((byte)0);
    }
}
