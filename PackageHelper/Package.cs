using System.Security;

namespace PackageHelper
{
    public static class Package
    {
        public const int ReservedSize = 15;
        public const int MaxPackageSize = 256;
        public const int MaxBodySize = MaxPackageSize - ReservedSize;

        public const byte Separator = 0x2F;
        public const byte EndByte = 0x0;

        public const int Fullness = 11;
        public const int Query = 12;
        public const int BodyStartIndex = ReservedSize;
        public const int CommandStart = 6;
        public const int CommandEnd = CommandStart + 3;

        //Protocol: #ANTP/****/**/...[END]

        public static readonly byte[] BasePackage =
        {
            0x23, 0x41, 0x4E, 0x50, Separator
        };

        public static byte[] GetContent(byte[] package, int packageLength)
        {
            return package.Take(new Range(BodyStartIndex, packageLength - 2)).ToArray();
        }
        
        public static byte[] CreatePackage(byte[] content, byte[] command, PackageFullness fullness, QueryType query)
        {
            return new PackageBuilder(content.Length)
                .SetCommand(command)
                .SetFullness(fullness)
                .SetQueryType(query)
                .SetContent(content)
                .Build();
        }

        public static List<byte[]> GetPackages(byte[] content, byte[] command, QueryType query)
        {
            var packages = new List<byte[]>();

            if (content.Length > MaxBodySize)
            {
                var chunks = content.Chunk(MaxBodySize).ToList();
                var chunksCount = chunks.Count;

                for(var i = 0; i < chunksCount; i++)
                {
                    if (i == chunksCount - 1)
                    {
                        packages.Add(CreatePackage(content, command, PackageFullness.Full, query));
                        break;
                    }
                    else
                    {
                        packages.Add(CreatePackage(content, command, PackageFullness.Partial, query));
                    }
                }
            }
            else
            {
                packages.Add(CreatePackage(content, command, PackageFullness.Full, query));
            }

            return packages;
        }

        public static bool IsQueryValid(byte[] buffer, int contentLength)
        {
            return default;
        }

        public static bool IsCreateSession(byte[] buffer)
        {
            return default;
        }

        public static bool IsJoin(byte[] buffer)
        {
            return default;
        }

        public static bool IsSayLetter(byte[] buffer)
        {
            return default;
        }

        public static bool IsSayWord(byte[] buffer)
        {
            return default;
        }

        public static bool IsPost(byte[] buffer)
        {
            return default;
        }

        public static bool IsMessage(byte[] buffer)
        {
            return default;
        }

        public static bool IsBye(byte[] buffer)
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
