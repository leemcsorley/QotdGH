using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    public class UserFollowTag
    {
        public virtual User SourceUser { get; set; }

        public virtual Guid SourceUserId { get; set; }

        public virtual Tag Tag { get; set; }

        public virtual Guid TagId { get; set; }

        public virtual bool LinksCreated { get; set; }
    }
}
