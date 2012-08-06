using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qotd.Entities;

namespace Qotd.PresentationObjects
{
    public class UserPO
    {
        public User User { get; set; }

        public int? UnreadNotifications { get; set; }

        public bool IsFollowedByCurrent { get; set; }

        public bool IsFollowingCurrent { get; set; }
    }
}
