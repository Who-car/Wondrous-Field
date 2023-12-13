namespace PackageHelper
{
    public class PackageBuilder
    {
        readonly byte[] _package;

        public PackageBuilder(int sizeOfContent)
        {
            if (sizeOfContent > Package.MaxBodySize)
            {
                throw new Exception();
            }

            _package = new byte[sizeOfContent + Package.ReservedSize];
            CreateBasePackage();
        }

        void CreateBasePackage()
        {
            Array.Copy(Package.BasePackage, _package, Package.BasePackage.Length);

            _package[Package.CommandEnd + 1] = Package.Separator;
            _package[Package.ReservedSize - 2] = Package.Separator;
            _package[^1] = Package.EndByte;
        }

        public PackageBuilder SetCommand(byte[] command)
        {
            for (var i = 0; i < Package.CommandEnd - Package.CommandStart + 1; i++)
            {
                _package[Package.CommandStart + i] = command[i];
            }

            return this;
        }

        public PackageBuilder SetFullness(PackageFullness fullness)
        {
            _package[Package.Fullness] = (byte)fullness;
            return this;
        }

        public PackageBuilder SetQueryType(QueryType type)
        {
            _package[Package.Query] = (byte)type;
            return this;
        }

        public PackageBuilder SetContent(byte[] content)
        {
            if (content.Length > _package.Length - Package.ReservedSize) throw new Exception();

            for(var i = 0; i < content.Length; i++)
            {
                _package[Package.ReservedSize - 1 + i] = content[i];
            }

            return this;
        }

        public byte[] Build()
        {
            return _package;
        }
    }
}
