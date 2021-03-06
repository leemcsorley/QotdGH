﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Entities
{
    [Flags]
    public enum FollowSource
    {
        UserFollow = 1,
        TagFollow = 2
    }

    public class UserFollow
    {
        public virtual User SourceUser { get; set; }

        public virtual Guid SourceUserId { get; set; }

        public virtual User TargetUser { get; set; }

        public virtual Guid TargetUserId { get; set; }

        public virtual bool LinksCreated { get; set; }
    }
}
