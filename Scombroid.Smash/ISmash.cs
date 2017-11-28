using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scombroid.Smash
{
    public interface ISmash
    {
        bool RunTest(int threadNo, int iteration);
    }
}
