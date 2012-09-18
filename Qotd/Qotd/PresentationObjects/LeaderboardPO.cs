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

        public int Skip { get; set; }

        public int Take { get; set; }

        public ScoreEntryType Type { get; set; }

        public bool DisplayHeaders { get; set; }
    }
}
