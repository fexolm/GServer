using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T
{
    class Program
    {
        static void Main(string[] args)
        {
            uint val = uint.MaxValue;
            for (int i = 0; i < 64; i++)
            {
                val = val << 1;
                Console.WriteLine("{0}\t\t{1}", i, val);
        }
                Console.WriteLine((uint)1 << 33);
            Console.ReadKey();
        }
    }
}
