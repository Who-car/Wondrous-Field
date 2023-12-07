namespace PackageHelper
{
    public static class Package
    {
        public const int MaxPackageSize = 256;
        public const int MaxContentSize = 240;
        public const int MaxFreeBytes = MaxPackageSize - MaxContentSize;

        public const byte Separator = 0x2F;
        public const byte End = 0x0;
        public static readonly (int, int) Command = (6, 9);
        public const int Fullness = 11;
        public const int Query = 12;

        //Protocol: #ANTP/****/**/...[END]

        public static readonly byte[] BasePackage =
        {
            0x23, 0x41, 0x4E, 0x50, Separator
        };

        public static byte[] GetContent(byte[] buffer, int contentLength)
        {
            return default!;
        }
        
        public static byte[] CreatePackage(byte[] content)
        {
            return default!;
        }

        public static List<byte[]> DivideIntoPackages()
        {
            return default!;
        }

        public static bool IsQueryValid(byte[] buffer, int contentLength)
        {
            return default;
        }

        public static bool IsHello(byte[] buffer)
        {
            return default;
        }

        public static bool IsSignUp(byte[] buffer)
        {
            return default;
        }

        public static bool IsSignIn(byte[] buffer)
        {
            return default;
        }

        public static bool IsJoin(byte[] buffer)
        {
            return default;
        }

        public static bool IsBye(byte[] buffer)
        {
            return default;
        }

        public static bool IsSay(byte[] buffer)
        {
            return default;
        }

        public static bool IsPartial(byte[] buffer)
        {
            return default;
        }

        public static bool IsFull(byte[] buffer)
        {
            return default;
        }
    }
}
