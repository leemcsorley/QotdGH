using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Qotd.Data;

namespace QotdRealTimeTester
{
    class Program
    {
        static void Main(string[] args)
        {
            QotdContext.UseLucene = false;
            Thread worker = new Thread(new ThreadStart(() => Qotd.WorkerImpl.WorkerUtils.ProcessAll()));
            worker.Start();
            int numTesters = 20;
            Tester[] testers = new Tester[numTesters];
            for (int i = 0; i < numTesters; i++)
            {
                testers[i] = new Tester(i);
                testers[i].Start();
            }
            Thread.Sleep(System.Threading.Timeout.Infinite);
        }
    }
}
