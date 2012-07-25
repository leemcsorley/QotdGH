using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qotd.Data;
using QotdMvc.Data;

namespace QotdDb
{
    class Program
    {
        static void Main(string[] args)
        {
            // Database and data
            using (QotdContext context = new QotdContext())
            {
                if (!context.Database.Exists())
                {
                    context.Database.Create();
                    TestData.Create(context);
                }
            }
        }
    }
}
