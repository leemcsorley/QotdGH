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
                Thread.Sleep(500);
                using (QotdContext db = new QotdContext())
                { 
                    db.CreateActivitiesAndNotifications();
                }
                using (QotdContext db = new QotdContext())
                {
                    db.CreateUserFollowLinksForNewFollows();
                }
                using (QotdContext db = new QotdContext())
                {
                    db.CreateUserFollowLinksForNewContent();
                }
                using (QotdContext db = new QotdContext())
                {
                    db.ProcessTags();
                }
                using (QotdContext db = new QotdContext())
                {
                    db.UpdateUserRankings();
                }
                using (QotdContext db = new QotdContext())
                {
                    db.AggregateNotifications();
                }
            }
        }
    }
}
