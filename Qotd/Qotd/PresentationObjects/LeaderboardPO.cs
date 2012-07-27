using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.PresentationObjects
{
    public class LeaderboardPO
    {
        public LeaderboardRowPO[] Top { get; set; }

        public LeaderboardRowPO[] AroundUser { get; set; }
    }
}
