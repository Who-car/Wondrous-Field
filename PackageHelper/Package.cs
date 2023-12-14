using System.Net.Sockets;
using System.Text;

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
            return package.Take(new Range(BodyStartIndex - 1, packageLength - 1)).ToArray();
        }

        public static async Task<ReceivedData> GetFullContent(Socket socket)
        {
            var content = new List<byte>();
            var buffer = new byte[MaxPackageSize];
            int packageLength;
            byte[] command = new byte[4];

            while (socket.Connected)
            {
                packageLength = await socket.ReceiveAsync(buffer, SocketFlags.None).ConfigureAwait(false);
                Console.WriteLine(Encoding.UTF8.GetString(buffer));
                if (!IsPackageValid(buffer, packageLength)) throw new Exception();

                command = GetCommand(buffer);
                content.AddRange(GetContent(buffer, packageLength));

                if (IsPartial(buffer))
                {
                    continue;
                }
                break;
            }

            return new ReceivedData { Body = content.ToArray(), Command = command };
        }

        public static byte[] GetCommand(byte[] package)
        {
            return package.Take(new Range(CommandStart, CommandEnd + 1)).ToArray();
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
                    packages.Add(CreatePackage(chunks[i], command, fullness, query));
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
                && package[ThirdSeparatorIndex].Equals(Separator);
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

        public static bool IsCreateSession(byte[] command)
        {
            return command.SequenceEqual(Command.CreateSession);
        }

        public static bool IsJoin(byte[] command)
        {
            return command.SequenceEqual(Command.Join);
        }

        public static bool IsNameLetter(byte[] command)
        {
            return command.SequenceEqual(Command.NameTheLetter);
        }

        public static bool IsNameWord(byte[] command)
        {
            return command.SequenceEqual(Command.NameTheWord);
        }

        public static bool IsPost(byte[] command)
        {
            return command.SequenceEqual(Command.Post);
        }

        public static bool IsMessage(byte[] command)
        {
            return command.SequenceEqual(Command.SendMessage);
        }

        public static bool IsBye(byte[] command)
        {
            return command.SequenceEqual(Command.Bye);
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

        public static async Task SendResponseToUser(Socket socket, byte[] content)
        {
            var packages = GetPackages(content, Command.Post, QueryType.Response);

            foreach (var package in packages)
            {
                await socket.SendAsync(package, SocketFlags.None);
            }
        }

        public static async Task SendContentToSocket(Socket socket, byte[] content, byte[] command, QueryType queryType)
        {
            if (socket.Connected)
            {
                var packages = GetPackages(content, command, queryType);

                foreach (var p in packages) await socket.SendAsync(p, SocketFlags.None).ConfigureAwait(false);
            }
            else throw new Exception();
        }
    }
}
