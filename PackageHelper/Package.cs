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
        public const int SecondSeparatorIndex = CommandEnd + 1;
        public const int ThirdSeparatorIndex = CommandEnd + 4;

        //Protocol: #ANTP/****/**/...[END]

        public static readonly byte[] BasePackage =
        {
            0x23, 0x41, 0x4E, 0x54, 0x50, Separator
        };

        public static byte[] GetContent(byte[] package, int packageLength)
        {
            return package.Take(new Range(BodyStartIndex, packageLength - 2)).ToArray();
        }

        public static byte[] GetCommand(byte[] package)
        {
            return package.Take(new Range(CommandStart, CommandEnd)).ToArray();
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

                for (var i = 0; i < chunksCount; i++)
                {
                    var fullness = PackageFullness.Partial;
                    if (i == chunksCount - 1)
                    {
                        fullness = PackageFullness.Full;
                    }
                    packages.Add(CreatePackage(content, command, fullness, query));
                }
            }
            else
            {
                packages.Add(CreatePackage(content, command, PackageFullness.Full, query));
            }

            return packages;
        }

        public static bool IsPackageValid(byte[] buffer, int packageLength)
        {
            return packageLength <= buffer.Length
                && HasProtocol(buffer)
                && HasCorrectCommand(buffer)
                && HasSeparators(buffer)
                && HasFullness(buffer)
                && HasQueryType(buffer)
                && HasEnd(buffer, packageLength);
        }

        public static bool HasProtocol(byte[] package)
        {
            return package[..CommandStart].SequenceEqual(BasePackage);
        }

        public static bool HasEnd(byte[] buffer, int packageLength)
        {
            return EndByte.Equals(buffer[packageLength - 1]);
        }

        public static bool HasCorrectCommand(byte[] package)
        {
            return Command.IsExistedCommand(package[CommandStart..(CommandEnd + 1)]);
        }

        public static bool HasSeparators(byte[] package)
        {
            return package[SecondSeparatorIndex].Equals(Separator)
                || package[ThirdSeparatorIndex].Equals(Separator);
        }

        public static bool HasFullness(byte[] package)
        {
            return package[Fullness] is (byte)PackageFullness.Full
                or (byte)PackageFullness.Partial;
        }

        public static bool HasQueryType(byte[] package)
        {
            return package[Query] is (byte)QueryType.Request
                or (byte)QueryType.Response;
        }

        public static bool IsCreateSession(byte[] buffer)
        {
            return buffer[CommandStart..(CommandEnd + 1)].SequenceEqual(Command.CreateSession);
        }

        public static bool IsJoin(byte[] buffer)
        {
            return buffer[CommandStart..(CommandEnd + 1)].SequenceEqual(Command.Join);
        }

        public static bool IsNameLetter(byte[] buffer)
        {
            return buffer[CommandStart..(CommandEnd + 1)].SequenceEqual(Command.NameTheLetter);
        }

        public static bool IsNameWord(byte[] buffer)
        {
            return buffer[CommandStart..(CommandEnd + 1)].SequenceEqual(Command.NameTheWord);
        }

        public static bool IsPost(byte[] buffer)
        {
            return buffer[CommandStart..(CommandEnd + 1)].SequenceEqual(Command.Post);
        }

        public static bool IsMessage(byte[] buffer)
        {
            return buffer[CommandStart..(CommandEnd + 1)].SequenceEqual(Command.SendMessage);
        }

        public static bool IsBye(byte[] buffer)
        {
            return buffer[CommandStart..(CommandEnd + 1)].SequenceEqual(Command.Bye);
        }

        public static bool IsPartial(byte[] buffer)
        {
            return buffer[Fullness] is (byte)PackageFullness.Partial;
        }

        public static bool IsFull(byte[] buffer)
        {
            return buffer[Fullness] is (byte)PackageFullness.Full;
        }

        public static bool IsRequest(byte[] buffer)
        {
            return buffer[Query] is (byte)QueryType.Request;
        }

        public static bool IsResponse(byte[] buffer)
        {
            return buffer[Query] is (byte)QueryType.Response;
        }
    }
}
