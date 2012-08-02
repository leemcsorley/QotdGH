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
            int loop = 0;
            while (true)
            {
                using (QotdContext db = new QotdContext())
                {
                    Thread.Sleep(500);
                        
                    db.CreateActivitiesAndNotifications();
                    db.CreateUserFollowLinks();

                    if (loop % 10 == 0)
                    {
                        db.UpdateUserRankings();
                        db.AggregateNotifications();
                    }
                }
                loop++;
                if (loop > 1000) 
                    loop = 0;
            }
        }
    }
}
