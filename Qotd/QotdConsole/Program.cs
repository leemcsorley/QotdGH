using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qotd.Data;
using System.Threading;
using Qotd.WorkerImpl;

namespace QotdConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            WorkerUtils.ProcessAll();
        }
    }
}
