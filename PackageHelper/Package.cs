namespace PackageHelper
{
    public static class Package
    {
        public const int MaxPacketSize = 256;
        public const int MaxContentSize = 240;
        public const int MaxFreeBytes = MaxPacketSize - MaxContentSize;

        public const byte Separator = 0x2F;
        public const byte End = 0x0;
        public static readonly (int, int) Command = (6, 9);
        public const int Fullness = 11;
        public const int Query = 12;

        public static readonly byte[] BasePackage =
        {
            0x2, 0x2, 0x41, 0x4E, 0x50, Separator
        };

        public static byte[] GetContent(byte[] buffer, int contentLength)
        {
            return default!;
        }

        public static bool IsValidQuery(byte[] buffer, int packageLength)
        {
            return default;
        }

        public static byte[] CreatePackage(byte[] content)
        {
            return default!;
        }

        public static List<byte[]> DivideIntoPackages()
        {
            return default!;
        }
    }
}
