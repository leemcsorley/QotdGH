using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public class UserActivityLink
    {
        public Guid UserId { get; set; }

        public User User { get; set; }

        public Guid ActivityId { get; set; }

        public Activity Activity { get; set; }
    }
}
