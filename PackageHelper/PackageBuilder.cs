using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        }
    }
}
