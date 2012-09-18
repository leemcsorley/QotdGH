using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qotd.Entities;

namespace Qotd.PresentationObjects
{
    public class LeaderboardRowPO
    {
        public User User { get; set; }

        public int Rank { get; set; }

        public int Score { get; set; }

        public int Stars { get; set; }

        public bool IsCurrentUser { get; set; }

        public int Ac { get; set; }

        public int Qv { get; set; }

        public int Qc { get; set; }

        public int A1 { get; set; }

        public int A2 { get; set; }

        public int A3 { get; set; }

        public int Q1 { get; set; }

        public int Av { get; set; }
    }
}
