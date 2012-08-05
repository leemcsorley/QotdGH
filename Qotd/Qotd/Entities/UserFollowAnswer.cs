using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public class UserFollowAnswer
    {
        public virtual User SourceUser { get; set; }

        public virtual Guid SourceUserId { get; set; }

        public virtual Answer Answer { get; set; }

        public virtual Guid AnswerId { get; set; }

        public virtual FollowSource Source { get; set; }
    }
}
