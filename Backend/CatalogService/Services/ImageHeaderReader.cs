using System.Buffers.Binary;

namespace CatalogService.Services;

internal static class ImageHeaderReader
{
    public static bool TryReadDimensions(ReadOnlySpan<byte> data, out int width, out int height)
    {
        width = 0;
        height = 0;
        if (data.Length < 12) return false;

        if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
        {
            return TryReadPng(data, out width, out height);
        }

        if (data[0] == 0xFF && data[1] == 0xD8)
        {
            return TryReadJpeg(data, out width, out height);
        }

        if (data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46 &&
            data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50)
        {
            return TryReadWebp(data, out width, out height);
        }

        return false;
    }

    private static bool TryReadPng(ReadOnlySpan<byte> data, out int width, out int height)
    {
        width = 0;
        height = 0;
        // 8-byte signature, then 4-byte length, then "IHDR" (4 bytes), then width(4) + height(4) big-endian
        if (data.Length < 24) return false;
        if (data[12] != 'I' || data[13] != 'H' || data[14] != 'D' || data[15] != 'R') return false;
        width = BinaryPrimitives.ReadInt32BigEndian(data.Slice(16, 4));
        height = BinaryPrimitives.ReadInt32BigEndian(data.Slice(20, 4));
        return width > 0 && height > 0;
    }

    private static bool TryReadJpeg(ReadOnlySpan<byte> data, out int width, out int height)
    {
        width = 0;
        height = 0;
        var i = 2;
        while (i + 9 < data.Length)
        {
            if (data[i] != 0xFF) return false;
            var marker = data[i + 1];
            i += 2;

            // Standalone markers (no length)
            if (marker == 0xD8 || marker == 0xD9 || (marker >= 0xD0 && marker <= 0xD7)) continue;

            if (i + 2 > data.Length) return false;
            var segmentLength = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(i, 2));

            // SOFn markers (0xC0-0xCF except 0xC4, 0xC8, 0xCC) carry frame dimensions
            if (marker >= 0xC0 && marker <= 0xCF && marker != 0xC4 && marker != 0xC8 && marker != 0xCC)
            {
                if (i + 7 > data.Length) return false;
                height = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(i + 3, 2));
                width = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(i + 5, 2));
                return width > 0 && height > 0;
            }

            i += segmentLength;
        }
        return false;
    }

    private static bool TryReadWebp(ReadOnlySpan<byte> data, out int width, out int height)
    {
        width = 0;
        height = 0;
        if (data.Length < 30) return false;
        // VP8 / VP8L / VP8X chunk starts at offset 12
        var format = data.Slice(12, 4);

        // VP8  (lossy, simple) — keyframe header at offset 23
        if (format[0] == 'V' && format[1] == 'P' && format[2] == '8' && format[3] == ' ')
        {
            if (data.Length < 30) return false;
            width = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(26, 2)) & 0x3FFF;
            height = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(28, 2)) & 0x3FFF;
            return width > 0 && height > 0;
        }

        // VP8L (lossless) — 14-bit width-1 / height-1 packed after 0x2F signature
        if (format[0] == 'V' && format[1] == 'P' && format[2] == '8' && format[3] == 'L')
        {
            if (data.Length < 25) return false;
            if (data[20] != 0x2F) return false;
            var b0 = data[21];
            var b1 = data[22];
            var b2 = data[23];
            var b3 = data[24];
            width = ((b1 & 0x3F) << 8 | b0) + 1;
            height = ((b3 & 0x0F) << 10 | b2 << 2 | (b1 & 0xC0) >> 6) + 1;
            return width > 0 && height > 0;
        }

        // VP8X (extended) — canvas dimensions are 24-bit little-endian, stored as value-1
        if (format[0] == 'V' && format[1] == 'P' && format[2] == '8' && format[3] == 'X')
        {
            if (data.Length < 30) return false;
            width = (data[24] | data[25] << 8 | data[26] << 16) + 1;
            height = (data[27] | data[28] << 8 | data[29] << 16) + 1;
            return width > 0 && height > 0;
        }

        return false;
    }
}
