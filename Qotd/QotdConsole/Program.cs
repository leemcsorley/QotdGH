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
            while (true)
            {
                using (QotdContext db = new QotdContext())
                {
                    Thread.Sleep(500);
                        
                    db.CreateActivitiesAndNotifications();
                }
            }
        }
    }
}
